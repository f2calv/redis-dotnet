# Redis with .NET

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=f2calv_redis-dotnet&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=f2calv_redis-dotnet)

A .NET 10 playground for Redis clients, hosted services, web applications, serialization,
benchmarks and tests.

## Projects

| Project | Purpose |
| --- | --- |
| `clientapp1`, `clientapp2` | Exercise Redis client patterns. |
| `serverapp` | Demonstrates Redis from a hosted process. |
| `webapi`, `webapp` | Demonstrate web integration and distributed caching. |
| `SharedLibrary` | Contains shared Redis and serialization behavior. |
| `SharedLibrary.Tests` | Contains unit tests for the shared library. |
| `BenchmarkApp` | Contains performance experiments. |

## Prerequisites

- A .NET 10 SDK
- Docker with Compose, when running local Redis services

## Run locally

Start the standalone Redis and web UI stack:

```pwsh
docker compose up -d
```

Redis listens on `localhost:6379` and the optional P3X Redis UI on `localhost:7843`.

Connect with the Redis CLI without relying on a generated container name:

```pwsh
docker compose exec redis redis-cli
```

The separate replication example is in `docker-compose.cluster.yml`:

```pwsh
docker compose -f .\docker-compose.cluster.yml up -d
```

Restore and build the .NET projects:

```pwsh
dotnet restore .\redis-dotnet.slnx
dotnet build .\redis-dotnet.slnx --no-restore
```

The Compose configurations use local unauthenticated development services. Do not expose them to an
untrusted network. Removing Compose volumes destroys their stored data.
