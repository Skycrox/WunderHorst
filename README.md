# WunderHorst

Ein selbst gehosteter Server mit Weboberfläche für die Verwaltung und den lokalen Betrieb von Tonieboxen – inspiriert von [TeddyCloud](https://github.com/toniebox-reverse-engineering/teddycloud).

> [!NOTE]
> WunderHorst befindet sich in einer frühen Entwicklungsphase. Grundlegende Infrastruktur (Dual-Port-Backend, Authentifizierung, Datenbank-Migrationen, Frontend-Grundgerüst und CI/CD) ist bereits vorhanden; fachliche Features folgen schrittweise.

---

## ✨ Architektur & Tech-Stack

WunderHorst setzt auf eine strikte Trennung zwischen Verwaltungs-Traffic und Geräte-Traffic sowie auf eine schlanke, reflektionsfreie Runtime:

- **Backend**: .NET 10 Minimal APIs mit **Native AOT** (`<PublishAot>true</PublishAot>`), **Vertical Slice Architecture**, `System.Text.Json` Source Generation, `Serilog` & `OpenTelemetry`
- **Datenhaltung**: SQLite (`Microsoft.Data.Sqlite`) mit **Dapper.AOT** und versionierten SQL-Migrationen
- **Frontend**: React 19, TypeScript, Vite, `pnpm`, **TanStack Query**, typsicherer `openapi-fetch`-Client (generiert aus der OpenAPI-Spezifikation des Backends), CSS Modules mit semantischen Design-Tokens (Light/Dark Theme)
- **Netzwerk-Zonen (Dual-Port Kestrel)**:
  - **Web/Admin-API** (Standard-Port `18420`): REST-API für das Frontend (`/api/health`, `/api/auth/*`), geschützt durch `HttpOnly`-Cookie-Sessions
  - **Boxine-API** (Standard-Port `18443`): Endpunkte für Toniebox-Geräteanfragen (z. B. `GET /boxine/ping`)

---

## 🚀 Schnellstart mit Docker Compose

Voraussetzung: [Docker](https://docs.docker.com/get-docker/) mit Docker Compose.

```bash
docker compose up --build -d
```

Anschließend ist WunderHorst unter folgenden Adressen erreichbar:

- **Weboberfläche (Frontend)**: `http://localhost:8080`
- **Web/Admin-API**: `http://localhost:18420`
- **Boxine-API**: `http://localhost:18443`

Beim ersten Aufruf der Weboberfläche führt der Setup-Assistent durch das einmalige Anlegen des initialen Admin-Kontos. Die SQLite-Datenbank wird persistent im Docker-Volume `wunderhorst_data` (`/data/wunderhorst.db`) gespeichert.

---

## 🛠️ Lokale Entwicklung

### Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (für Native-AOT-Publish unter Linux zusätzlich `clang` und `zlib1g-dev`)
- [Node.js 24+](https://nodejs.org/) und [pnpm](https://pnpm.io/)

### Backend starten & testen

```bash
# Abhängigkeiten wiederherstellen & bauen
dotnet restore WunderHorst.sln
dotnet build WunderHorst.sln --no-restore

# Tests ausführen
dotnet test WunderHorst.sln --no-build

# Entwicklungsserver starten (Standard-Ports: 18420 & 18443)
dotnet run --project src/backend/WunderHorst.Server/WunderHorst.Server.csproj
```

### Frontend starten & testen

```bash
cd src/frontend

# Abhängigkeiten installieren
pnpm install --frozen-lockfile

# Entwicklungsserver starten
pnpm run dev

# OpenAPI-Typen aus src/api/openapi.json neu generieren
pnpm run api:generate

# Lint (ESLint + Stylelint), Typecheck, Tests & Build
pnpm run lint
pnpm run typecheck
pnpm run test
pnpm run build
```

---

## 📂 Projektstruktur

```text
.
├── .github/workflows/       # CI-Pipeline (Tests, 100%-Branch-Coverage-Gate, Native AOT, Frontend)
├── docs/                    # Technische Dokumentation
├── src/
│   ├── backend/
│   │   └── WunderHorst.Server/
│   │       ├── Features/        # Vertical Slices (Auth, System, ...)
│   │       └── Infrastructure/  # DB, Migrationen, Auth, Port-Zonen, Observability
│   └── frontend/
│       └── src/
│           ├── api/             # OpenAPI-Schema & typisierter Client
│           ├── app/             # Root-Komposition & Provider
│           ├── features/        # Fachliche UI-Features
│           └── ui/              # Design-System-Komponenten & CSS-Tokens
├── tests/
│   └── WunderHorst.Server.Tests/
├── AGENTS.md                # Projekt-, Coding- und Commit-Richtlinien
├── compose.yaml             # Docker-Compose-Setup (Backend + Frontend)
└── WunderHorst.sln
```

---

## 📚 Dokumentation

Die vollständige technische Dokumentation befindet sich im Verzeichnis [`docs/`](docs/index.md):

- [Systemarchitektur – Übersicht](docs/architecture/overview.md)
- [Coding Guidelines](docs/development/coding-guidelines.md)
- [Projekt- & Commit-Richtlinien](AGENTS.md)

---

## 💡 Inspiration & Referenzen

WunderHorst lässt sich von [TeddyCloud](https://github.com/toniebox-reverse-engineering/teddycloud) (aus der Toniebox-Reverse-Engineering-Community) inspirieren. Viele Ideen rund um den lokalen Betrieb und die Verwaltung von Tonieboxen stammen aus diesem Projekt.

---

## 📄 Lizenz

Dieses Projekt steht unter der **GNU Affero General Public License v3.0 (AGPL-3.0)** – Details siehe [`LICENSE`](LICENSE).
