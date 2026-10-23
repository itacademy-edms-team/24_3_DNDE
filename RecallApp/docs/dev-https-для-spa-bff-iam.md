# Рабочий способ для настройки dev-https внутри и снаружи Docker-окружения

Данное руководство будет полезно пользователям Windows, которые ведут работу с Docker через WSL и устали от постоянных проблем с https для dev-разработки. Оно не универсально, однако хорошо закрывает проблемы связки `spa-bff-iam`.

При попытке запустить связку SPA (React) + BFF (.NET) + Keycloak на локальной машине без настройки HTTPS возникают три критические проблемы:
1. Проблема двойного адреса Keycloak (Mismatched Issuer URL): Для протокола OpenID Connect (OIDC) критически важно, чтобы адрес Keycloak (Issuer) был абсолютно одинаковым как для внешнего браузера, так и для бэкенда (BFF). Если пользователь заходит в Keycloak через браузер по адресу https://auth.localhost, а BFF под капотом обращается к нему по внутреннему имени Docker-сети http://iam:8080 (для обмена кода на токен), Keycloak заблокирует запрос из-за несовпадения Issuer. Оба участника должны видеть Keycloak по единому HTTPS-адресу.
2. Требование по безопасности Cookies (Secure, SameSite): Паттерн BFF подразумевает, что бэкенд сохраняет выданные токены в зашифрованную HttpOnly куку и отдает её в браузер. Для защиты от CSRF-атак современный стандарт требует выставлять флаги SameSite=Lax; Secure. Флаг Secure принудительно требует наличия HTTPS-соединения, иначе браузер просто такую куку проигнорирует.
3. Цепочка доверия SSL внутри Docker: Даже если выдать контейнерам самоподписанные SSL-сертификаты, код внутри контейнера BFF заблокирует любой запрос к Keycloak с ошибкой `AuthenticationException: The remote certificate is invalid`. Контейнеры как-то должны доверять одному и тому же корневому центру сертификации (Root CA) внутри Docker. Этот же центр сертификации нужно будет доставлять разработчику на машину, чтобы Windows не браковала выданные контейнерам сертификаты.

Данное решение, на мой взгляд, предоставляет очень удобное решение для проблем выше. С учётом выбранного стека.

## Архитектура решения

Вместо использования грязных хаков в коде (таких как отключение проверки SSL в C# через DangerousAcceptAnyServerCertificateValidator), мы строим изолированную замкнутую экосистему внутри Docker с использованием принципа локального удостоверения.


Ключевые компоненты решения:

- Единая точка входа (Reverse Proxy): Контейнер с прокси-сервером слушает внешние порты 80 и 443. Он принимает весь трафик (включая внутренний трафик между контейнерами), расшифровывает HTTPS и передает его конечным сервисам по HTTP внутри изолированной сети.
- Сетевые псевдонимы (Docker Network Aliases): Мы назначаем доменные имена (app.localhost, bff.localhost, auth.localhost) как alias-адреса для контейнера Reverse Proxy. Благодаря этому, когда BFF делает запрос на https://auth.localhost, Docker-сеть маршрутизирует этот запрос не в интернет, а на локальный прокси-сервер.
- Общий Root CA Volume: Корневой сертификат пробрасывается в контейнер BFF и в операционную систему Windows. Цепочка доверия становится непрерывной.

## Пошаговое решение

### Вариант А: Реализация на Nginx

Для Nginx нам понадобится дополнительный легковесный шаг генерации сертификатов при старте Docker окружения.

1. Напишем конфигурационный файл Nginx, `nginx.conf`:

Создайте файл nginx.conf в папке reverse-proxy проекта:
```
events { worker_connections 1024; }

http {
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # WebSocket-upgrade: нужен для Vite HMR сейчас и SignalR позже
    map $http_upgrade $connection_upgrade {
        default upgrade;
        ''      close;
    }

    # Общие прокси-заголовки
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Forwarded-Host $host;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $connection_upgrade;

    # 1. SPA (dev-сервер Vite)
    server {
        listen 443 ssl;
        http2 on;
        server_name app.localhost;
        ssl_certificate /etc/nginx/certs/server.crt;
        ssl_certificate_key /etc/nginx/certs/server.key;

        location / {
            proxy_pass http://spa:5173; # порт dev-сервера Vite по умолчанию
        }
    }

    # 2. BFF
    server {
        listen 443 ssl;
        http2 on;
        server_name bff.localhost;
        ssl_certificate /etc/nginx/certs/server.crt;
        ssl_certificate_key /etc/nginx/certs/server.key;

        location / {
            proxy_pass http://bff:5000;
        }
    }

    # 3. IAM (Keycloak)
    server {
        listen 443 ssl;
        http2 on;
        server_name auth.localhost;
        ssl_certificate /etc/nginx/certs/server.crt;
        ssl_certificate_key /etc/nginx/certs/server.key;

        location / {
            proxy_pass http://iam:8080;
            # Увеличенные буферы под тяжёлые OIDC-токены Keycloak
            proxy_buffer_size          128k;
            proxy_buffers              4 256k;
            proxy_busy_buffers_size    256k;
        }
    }

    # Перенаправление с HTTP на HTTPS
    server {
        listen 80;
        server_name app.localhost bff.localhost auth.localhost;
        return 301 https://$host$request_uri;
    }
}
```

> **Про Vite HMR за прокси.** Одних `Upgrade`/`Connection`-заголовков мало: Vite-клиент по умолчанию стучится ws-ом на свой origin и порт. Чтобы HMR ходил через `https://app.localhost`, в `vite.config` задать `server.hmr` (`protocol: 'wss'`, `host: 'app.localhost'`, `clientPort: 443`) и `server.host: true`. Без этого горячая перезагрузка молча отвалится, хотя статика грузится.

Для Bff-контейнера потребуется написать startup-скрипт, который будет принудительно обновлять информацию об имеющихся сертификатах. По умолчанию контейнеры так не делают, даже если пробросили сертификат или rootCA.

2. Напишем `entrypoint.sh` для для Bff-контейнера:

```
#!/bin/bash
# Проверяем, существует ли корневой УЦ
if [ -f /etc/nginx/certs/rootCA.pem ];
then # Если нет, копируем в контейнер из shared_volume rootCA и сертификат
    cp /etc/nginx/certs/rootCA.pem /usr/local/share/ca-certificates/local-dev-ca.crt
    update-ca-certificates
fi

exec dotnet YourBffProject.dll
```

3. Напишем корневой `docker-compose.yml` для всего проекта:

```
networks:
  dev_net:
    driver: bridge

volumes:
  shared_certs:
  iam_db_data:

services:
  # Автоматический генератор локального Root CA + wildcard-серта *.localhost
  ca-generator:
    image: alpine
    entrypoint: >
      sh -c "apk add --no-cache openssl &&
             mkdir -p /certs &&
             [ ! -f /certs/rootCA.key ] && openssl genrsa -out /certs/rootCA.key 2048 || true &&
             [ ! -f /certs/rootCA.pem ] && openssl req -x509 -new -nodes -key /certs/rootCA.key -sha256 -days 1024 -out /certs/rootCA.pem -subj '/CN=LocalDevCA' || true &&
             openssl genrsa -out /certs/server.key 2048 &&
             openssl req -new -key /certs/server.key -out /certs/server.csr -subj '/CN=*.localhost' &&
             openssl x509 -req -in /certs/server.csr -CA /certs/rootCA.pem -CAkey /certs/rootCA.key -CAcreateserial -out /certs/server.crt -days 365 -sha256"
    volumes:
      - shared_certs:/certs

  edge-proxy:
    image: nginx:alpine
    depends_on:
      ca-generator:
        condition: service_completed_successfully
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - shared_certs:/etc/nginx/certs:ro
    networks:
      dev_net:
        aliases:
          - app.localhost
          - bff.localhost
          - auth.localhost

  # Отдельный Postgres под Keycloak (ADR 0026): своя схема IAM, не доменная БД
  iam-db:
    image: postgres:17
    environment:
      - POSTGRES_DB=keycloak
      - POSTGRES_USER=keycloak
      - POSTGRES_PASSWORD=keycloak
    volumes:
      - iam_db_data:/var/lib/postgresql/data
    networks:
      - dev_net

  iam:
    image: quay.io/keycloak/keycloak:26.8.1
    command: start-dev
    depends_on:
      - iam-db
    environment:
      - KC_DB=postgres
      - KC_DB_URL=jdbc:postgresql://iam-db:5432/keycloak
      - KC_DB_USERNAME=keycloak
      - KC_DB_PASSWORD=keycloak
      - KC_HOSTNAME=https://auth.localhost
      - KC_HTTP_ENABLED=true
      - KC_PROXY_HEADERS=xforwarded
    networks:
      - dev_net

  bff:
    image: /your/bff/image:version
    depends_on:
      - iam
    environment:
      # realm публичного приложения (ADR 0004), не master
      - Authentication__Oidc__Authority=https://auth.localhost/realms/recallapp
    volumes:
      - .:/app
      - shared_certs:/etc/nginx/certs:ro
    working_dir: /app
    entrypoint: ["/bin/bash", "./entrypoint.sh"]
    networks:
      - dev_net

  spa:
    image: node:alpine
    # Dev-сервер Vite, слушать 0.0.0.0:5173 (vite --host)
    networks:
      - dev_net

```

4. Поднимаем Docker-групу:
```
docker compose up -d
```

5. Импорт центра сертификации в Windows.
   1. Скачайте из volume `shared_certs` файл `rootCA.pem` в Windows.
   2. Дважды нажмите на данный файл -> установить сертификат -> Локальный компьютер -> Поместить все сертификаты в следующее хранилище: Доверенные корневые центры сертификации (Trusted Root Certification Authorities).

## Послесловие

По окончании работы не забыть удалить доверие к локальному Root CA из хранилища Windows (п.5): dev-CA не должен оставаться в системе и тем более попадать в прод.