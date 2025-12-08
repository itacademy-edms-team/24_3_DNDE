# FinanceTrack.Finance

## Requirements

- Docker
- Docker compose

## Setup

### Setup `.env`

1. Create file `.env.dev`
2. Move all data from `.env.example` to `.env.dev`
3. Make changes to data in `.env.dev`

## Launch

### Run containers

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml up -d
```

### Stop containers

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml stop
```

### Stop and Delete containers

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml down
```
