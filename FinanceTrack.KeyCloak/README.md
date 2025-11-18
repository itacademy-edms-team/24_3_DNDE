# FinanceTrack.KeyCloak

## Requirements

- Docker
- Docker compose

## Setup

### Setup `.env`

1. Create file `.env.dev`
2. Move all data from `.env.example` to `.env.dev`
3. Make changes to data in `.env.dev`

### Setup folders

```bash
mkdir backups
mkdir exports
```

## Launch

### Run containers

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml up -d
```

Check admin panel at `http://localhost:${KC_PUBLIC_PORT}`

### Stop containers

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml stop
```

### Stop and Delete containers

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml down
```

## Export Realms

1. Stop KeyCloak container
```bash
docker stop FinanceTrack-keycloak
```
2. Run command
```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml run --rm keycloak export --dir=/temp/exports
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
docker compose --env-file .env.dev -f docker-compose.dev.yml run --rm keycloak import --dir=/temp/exports
```
3. Run KeyCloak container
```bash
docker start FinanceTrack-keycloak
```
4. Go to admin panel and make sure that realms imported
