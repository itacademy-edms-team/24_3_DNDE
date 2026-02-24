## Requirements

- Docker
- Docker compose

## Setup

Make sure that ports in `docker-compose.override.yml` are available.

## Launch

Launch and build container:
```bash
docker compose up -d --build
```

Launch container using hot reload features:
```bash
docker compose watch
```
