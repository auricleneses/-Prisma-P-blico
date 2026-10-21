# Prisma Público

A source-linked public case browser and minimal ASP.NET Core API. The reader page is public. Creating, editing, and deleting records requires the `X-Admin-Key` header; without `PRISMA_ADMIN_KEY`, write requests are denied.

The case list is a sourced selection, not an exhaustive catalog of every allegation. Allegations, investigations, convictions, annulments, and acquittals must remain distinct. Review every source and update a record when a case status changes.

## Run Locally

With .NET 10 installed, run from this directory:

```powershell
dotnet run --project .\NihonCheck.csproj
```

Open the local address printed by `dotnet run`. The API is available at `/api/evidencias`; Swagger is at `/swagger`.

The application writes `data/evidencias.json` by default. To choose another data folder, set `PRISMA_DATA_DIR` before starting it.

## Run With Docker

Install Docker Desktop, then run these commands in PowerShell:

```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$env:PRISMA_ADMIN_KEY = [BitConverter]::ToString($bytes).Replace('-', '').ToLowerInvariant()
docker compose up --build
```

Open `http://localhost:5088`. Compose stores records in the named `prisma-data` volume. Keep `PRISMA_ADMIN_KEY` private; do not put it in browser code or commit it to source control.

Write API requests must include the secret header:

```powershell
-Headers @{ 'X-Admin-Key' = $env:PRISMA_ADMIN_KEY }
```

## Public Hosting On Render

1. Push this project to a private or public GitHub repository.
2. In Render, choose **New > Blueprint**, connect the repository, and deploy the included `render.yaml`.
3. When prompted, set `PRISMA_ADMIN_KEY` to a new, unique secret. Do not reuse the local key.
4. Render will provide a public `onrender.com` URL. Its health check is `/api/status`; the reader page is `/`.

The included Render configuration uses a Starter web service and a 1 GB persistent disk mounted at `/data`. Render's persistent disk is a paid feature. Do not switch to an ephemeral filesystem if you want records to survive deploys or restarts. Back up `data/evidencias.json` regularly.

## Admin API

The write routes are `POST /api/evidencias`, `PUT /api/evidencias/{id}`, and `DELETE /api/evidencias/{id}`. Each requires `X-Admin-Key`. Public readers can use `GET /api/evidencias` and `GET /api/evidencias/{id}` without the key.
