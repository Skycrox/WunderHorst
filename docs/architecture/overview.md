# Systemarchitektur – WunderHorst

Dieses Dokument beschreibt die grundlegende Architektur von **WunderHorst** (Backend, Frontend, Datenhaltung, Sicherheit und Container-Setup).

---

## 1. Dual-Port Kestrel & Netzwerk-Zonen

Der Backend-Server (`WunderHorst.Server`, .NET 10 Native AOT) betreibt zwei logisch getrennte Kestrel-Endpunkte, um Verwaltungs- und Geräte-Traffic strikt zu isolieren:

| Zone | Konfigurationsschlüssel | Standard-Port | Zweck |
| :--- | :--- | :--- | :--- |
| **Web/Admin-API** | `Server:WebApiPort` | `18420` | REST-API für das Frontend (`/api/health`, `/api/auth/*`, künftige Verwaltungs-Slices), geschützt durch Cookie-Authentifizierung |
| **Boxine-API** | `Server:BoxineApiPort` | `18443` | Endpunkte für Toniebox-Geräteanfragen (z. B. `GET /boxine/ping`) |

Die Zonen-Trennung wird über `RequireHost` / Port-Filterung in ASP.NET Core Minimal APIs durchgesetzt (`Infrastructure/Http/PortZoneFilter.cs`). Anfragen an Endpunkte einer anderen Zone werden abgewiesen.

---

## 2. Backend-Architektur (Vertical Slice Architecture & Native AOT)

### Vertical Slice Architecture (VSA)
Jede fachliche Funktion liegt in einem eigenen Slice unter `src/backend/WunderHorst.Server/Features/<Slice>/`:
- **Endpoint** – Minimal-API-Route, OpenAPI-Metadaten, Zonen-Zuweisung
- **Handler / Service** – Geschäftslogik (direkt testbar ohne HTTP-Host)
- **Validator / Mapper / Models** – Eingabevalidierung und DTOs

### Native AOT & JSON
Das Projekt kompiliert mit `<PublishAot>true</PublishAot>` ohne Reflection zur Laufzeit:
- JSON-Serialisierung erfolgt vollständig über `System.Text.Json` Source Generation (`WunderHorstJsonContext`).
- Konfigurations-Binding nutzt den .NET Configuration Binding Source Generator (`<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>`).
- Logging und Observability basieren auf `Serilog` (Code-Konfiguration ohne Reflection-AppSettings-Scanner) und `OpenTelemetry`.

---

## 3. Datenhaltung (SQLite & Dapper.AOT)

- **Engine**: SQLite über `Microsoft.Data.Sqlite` (Standard-Verbindungszeichenfolge `Data Source=data/wunderhorst.db`, im Container `/data/wunderhorst.db`).
- **Datenzugriff**: `Dapper` + `Dapper.AOT` generieren SQL-Mappings und Command-Code zur Compile-Zeit ohne dynamische IL-Emission.
- **Migrationen**: Versionierte SQL-Skripte sind als Embedded Resources unter `Infrastructure/Database/Migrations/` abgelegt und werden beim Serverstart idempotent über die Tabelle `schema_migrations` ausgeführt.

---

## 4. Authentifizierung & Session-Verwaltung

- **Initiales Setup**: Solange kein Benutzer existiert (`GET /api/auth/status` → `needsSetup: true`), erlaubt `POST /api/auth/setup` das einmalige Anlegen des initialen Admin-Kontos.
- **Passwort-Hashing**: PBKDF2-HMAC-SHA256 (`Rfc2898DeriveBytes`) mit zufälligem Salt und festem Iterationszähler.
- **Sessions**: Sign-In (`POST /api/auth/login`) erzeugt einen kryptografisch zufälligen Session-Token, der als SHA-256-Hash in SQLite gespeichert und als `HttpOnly`-Cookie (`Auth:CookieName`, Standard `wunderhorst_session`, konfigurierbares `Secure`-Flag via `Auth:RequireSecureCookie`) ausgeliefert wird.
- **Logout**: `POST /api/auth/logout` invalidiert die Session in der Datenbank und löscht das Cookie.

---

## 5. Frontend-Architektur (`src/frontend`)

- **Stack**: React 19, TypeScript, Vite, TanStack Query (`@tanstack/react-query`), `openapi-fetch`.
- **OpenAPI → TypeScript Pipeline**:
  1. Beim Backend-Build wird `src/frontend/src/api/openapi.json` über `Microsoft.Extensions.ApiDescription.Server` erzeugt.
  2. `pnpm run api:generate` generiert daraus `src/frontend/src/api/schema.d.ts` mittels `openapi-typescript`.
  3. `src/api/client.ts` stellt einen typisierten `openapi-fetch`-Client bereit.
- **Schichtenmodell (ESLint `eslint-plugin-boundaries`)**:
  - `src/ui/` – Design-System-Komponenten (`Button`, `Input`, `Card`, `Alert`, `Badge`, `AppShellLayout`) und semantische CSS-Tokens (`src/ui/theme/tokens.css`, Light/Dark via `[data-theme]`).
  - `src/features/` – Fachliche Features (`auth`, `app-shell`), die ausschließlich Komponenten aus `src/ui/` komponieren (rohe HTML-Elemente und direkte CSS-Imports sind per ESLint untersagt).
  - `src/app/` – Root-Komposition (`App.tsx` mit `QueryClientProvider`, `ThemeProvider`, `AuthProvider`).

---

## 6. Container- & Entwicklungs-Setup

- **Docker**:
  - `src/backend/WunderHorst.Server/Dockerfile`: Multi-Stage-Build mit `mcr.microsoft.com/dotnet/sdk:10.0-noble-aot` und minimalem `runtime-deps:10.0-noble-chiseled-extra` Runtime-Image.
  - `src/frontend/Dockerfile`: Multi-Stage-Build mit `node:24-alpine` (`pnpm`) und `nginx:alpine` (SPA-Fallback + `/api/`-Proxy auf `backend:18420`).
  - `compose.yaml`: Orchestriert `backend` (`18420`, `18443`, Volume `wunderhorst_data:/data`) und `frontend` (`8080:80`).
- **VS Code Tasks**: `.vscode/tasks.json` stellt Tasks für Docker Compose (`Up`/`Down`/`Stop` zusammen und getrennt), Dev-Server (`dotnet watch` + `pnpm dev`) sowie Builds bereit.
- **CI**: `.github/workflows/ci.yml` prüft Backend-Tests inkl. 100 % Branch-Coverage-Gate auf Feature-Klassen, Native-AOT-Publish sowie Frontend Lint, Typecheck, Tests und Build.
