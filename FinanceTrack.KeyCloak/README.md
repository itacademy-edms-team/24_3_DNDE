# FinanceTrack.KeyCloak

## Requirements

- Docker
- Docker compose

## Setup

### Setup `.env`

1. Create file `.env.development`
2. Move all data from `.env.example` to `.env.development`
3. Make changes to data in `.env.development`

### Setup folders

```bash
mkdir backups
mkdir exports
```

## Launch

### Run containers

```bash
docker compose --env-file .env.development -f docker-compose.development.yml up -d
```

Check admin panel at `http://localhost:${KC_PUBLIC_PORT}`

### Stop containers

```bash
docker compose --env-file .env.development -f docker-compose.development.yml stop
```

### Stop and Delete containers

```bash
docker compose --env-file .env.development -f docker-compose.development.yml down
```

## Export Realms

1. Stop KeyCloak container
```bash
docker stop FinanceTrack-keycloak
```
2. Run command
```bash
docker compose --env-file .env.development -f docker-compose.development.yml run --rm keycloak export --dir=/temp/exports
```
3. Files appear in `exports` folder
4. Run KeyCloak container
```bash
docker start FinanceTrack-keycloak
```

### Import Realm

1. Stop KeyCloak container
```bash
docker stop FinanceTrack-keycloak
```
2. Run command
```bash
docker compose --env-file .env.development -f docker-compose.development.yml run --rm keycloak import --dir=/temp/exports
```
3. Run KeyCloak container
```bash
docker start FinanceTrack-keycloak
```
4. Go to admin panel and make sure that realms imported
