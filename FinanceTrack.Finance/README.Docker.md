# FinanceTrack.Finance

## Requirements

- Docker
- Docker compose

## Setup

Before launch you need to create propper .env file. See commands bellow for reference and docker-compose file.

## Launch

There are two launch options: using Visual Studio laucnh profile and using docker-compose dirrectly

All file actions made in root dirrectory (.sln file location)

### Launch in Visual Studio

1. Open project solution file.
2. Then, set all blank lines in appsettings.json to UserSecrets
3. Create and fill .env.development (see .env.example for reference)
4. Launch project using Docker Compose launch profile

### Launch using Docker-compose dirrectly

1. Create and fill .env.development (see .env.example for reference)
2. Use launch command bellow:
```bash
docker compose --env-file .env.development up -d --build
```
This command build images and run containers.

Other commands listed bellow.

Check final configuration:
```bash
docker compose --env-file .env.development config
```

Launch without rebuild:
```bash
docker compose --env-file .env.development up -d
```

Stop containers:
```bash
# All services
docker compose --env-file .env.development stop

# Service-specified
docker compose --env-file .env.development stop financetrack.finance.web
```

Restart containers:
```bash
# All services
docker compose --env-file .env.development restart

# Service-specified
docker compose --env-file .env.development restart financetrack.finance.web
```

Stop and delete containers (without touching volumes):
```bash
docker compose --env-file .env.development down
```
