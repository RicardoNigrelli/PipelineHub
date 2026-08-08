# PipelineHub

A background-job orchestrator built in **ASP.NET Core (.NET 10)** with **Clean Architecture**,
**MediatR/CQRS**, **FluentValidation**, and **Serilog** — the stack that keeps showing up
across real .NET job postings (EF Core, Hangfire, SignalR, Docker, React frontends).

This isn't a tutorial clone. It's a real tool: it queues and runs media-processing jobs
(ffmpeg transcodes today; whisper transcription and Remotion renders coming in later
phases) with retries and live progress, instead of the synchronous one-off scripts that
kick off that kind of work by default.

## Status: Phase 2 — persisted end to end with EF Core + Postgres

- `POST /jobs` enqueues a job (validated, 400 on bad input)
- `GET /jobs/{id}` returns its status — survives an API restart, backed by Postgres
- `POST /jobs/enqueue-and-wait` runs synchronously and returns the final result — handy
  for testing before Hangfire (background queueing) lands in Phase 4
- One real runner ships today: `SampleFfmpegTranscode`, which resizes the repo's own
  `assets/sample.mp4` with ffmpeg — works out of the box on a fresh clone, no external
  dependencies or private data required.
- EF Core migrations apply automatically on startup in dev (`Database.Migrate()`).

## Architecture

```
src/
├── PipelineHub.Domain          # Job, JobStatus, JobType — no dependencies
├── PipelineHub.Application     # MediatR commands/queries, FluentValidation,
│                                  IJobRunner / IJobRepository ports
├── PipelineHub.Infrastructure  # EF Core + Postgres (EfJobRepository), SampleFfmpegJobRunner
└── PipelineHub.Api             # Minimal API endpoints, Serilog, DI wiring
```

`IJobRunner` is the seam that keeps this repo public-safe: any job type that touches
private media (e.g. a future video-lab/reel-lab adapter) gets registered through local,
gitignored configuration — never committed here.

## Running locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download), `ffmpeg` on PATH, and
Docker (for Postgres).

```bash
docker compose up -d db   # host port 15434, to avoid clashing with other local projects on 5432
dotnet build
dotnet run --project src/PipelineHub.Api   # applies EF Core migrations on startup
```

```bash
curl -X POST http://localhost:5299/jobs/enqueue-and-wait \
  -H "Content-Type: application/json" \
  -d '{"type":"SampleFfmpegTranscode","parameters":{"width":"160"}}'
```

Migrations (via the local `dotnet-ef` tool, see `.config/dotnet-tools.json`):

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add <Name> --project src/PipelineHub.Infrastructure --startup-project src/PipelineHub.Api -o Persistence/Migrations
```

## Roadmap

1. ~~Setup: solution, Docker Compose, CI~~
2. ~~Domain + Application core, in-memory runner~~
3. ~~Persistence: EF Core + Postgres, job history~~
4. Background processing: Hangfire (queue, retries)
5. Real-time: SignalR progress updates
6. Real adapters: video-lab/reel-lab (local config, not public)
7. Frontend: React dashboard
8. Observability: structured logging, health checks, integration tests
9. Deploy: Docker Compose + AWS
