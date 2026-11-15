# Coding Guidelines (WunderHorst)

> Schlüsselwörter: **MUSS / DARF NICHT** = verbindlich, **SOLLTE** = Standard, Abweichung nur mit Begründung.
>
> Diese Richtlinien gelten für die gesamte Codebasis in `WunderHorst/` und ergänzen die Projekt-, Git- und Dokumentationsregeln in [`AGENTS.md`](../../AGENTS.md).

---

## 1. Tech-Stack

| Bereich | Technologie |
| :--- | :--- |
| **Backend** | C# / .NET 10, ASP.NET Core Minimal APIs, **Native AOT** |
| **Datenbank** | SQLite (`Microsoft.Data.Sqlite`) + **Dapper.AOT** (Source-Generator-basiert) |
| **Observability** | Serilog (Structured Logging), OpenTelemetry (Metrics & Traces), `/health` |
| **API-Contract** | OpenAPI (Backend) → automatisch generierter TypeScript-Client (Frontend) |
| **Frontend** | React + Vite + TypeScript (SPA, kein SSR), **`pnpm`** |
| **Styling & Theming** | **CSS Modules** (`*.module.css`) + CSS Custom Properties (Design Tokens) |
| **Data Fetching / State** | **TanStack Query** (Server State), React `useState`/`useReducer`/Context (UI State) |

---

## 2. Backend-Richtlinien (.NET 10, Minimal APIs & Native AOT)

### 2.1 Vertical Slice Architecture (VSA)
* Code **MUSS** feature-basiert organisiert werden (`Features/<Domain>/<FeatureName>/`).
* Alles, was zu einem Use-Case gehört (Endpoint-Registrierung, Request/Response-Records, Validierung, Handler, SQL-Query, Mapper-Extensions), **MUSS** zusammen im jeweiligen Slice-Ordner liegen.
* `Shared/` bzw. `Infrastructure/` **DARF** ausschließlich für echte Querschnittsthemen genutzt werden (z. B. Auth, Datenbank-Verbindung, Error-Handling/`ProblemDetails`, Logging/Telemetry, Port-Filter).
* Es **DÜRFEN KEINE** künstlichen `Service`- oder `Repository`-Schichten angelegt werden, die nur Aufrufe durchreichen.

### 2.2 Native-AOT-Kompatibilität (Strikt)
* Das Backend **MUSS** ohne Warnungen mit Native AOT publizierbar sein (`dotnet publish`).
* **Verbot von Reflection:**
  * JSON-Serialisierung **MUSS** über `System.Text.Json` Source Generation (`JsonSerializerContext` mit `[JsonSerializable]`) erfolgen.
  * Konfigurationen **MÜSSEN** über `Microsoft.Extensions.Configuration` mit `<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>` gebunden werden.
  * Reflection-basierte Mapper (z. B. AutoMapper) oder Reflection-basierte Validatoren **DÜRFEN NICHT** verwendet werden.

### 2.3 Mapper & Validierung
* Mapping zwischen Domain-/DB-Modellen und Request/Response-DTOs **MUSS** über explizite **Extension Methods** geschrieben werden (z. B. `OrderMapperExtensions.ToResponse()`).
* Fehlerantworten **MÜSSEN** einheitlich als `ProblemDetails` (RFC 9457) zurückgegeben werden.

### 2.4 Datenbank (SQLite & Dapper.AOT)
* Datenbankzugriffe **MÜSSEN** über `Microsoft.Data.Sqlite` und **Dapper.AOT** (`[DapperAot]`) erfolgen; klassisches EF Core / LiteDB **DARF NICHT** verwendet werden.
* Schema-Änderungen **MÜSSEN** über versionierte SQL-Migrationen erfolgen.

### 2.5 Sicherheit, Port-Zonen & Streaming
* **Getrennte Port-Zonen:** `WebApi` (Web-UI & Admin-Endpoints) und `BoxineApi` (Toniebox-Endpoints) **MÜSSEN** in derselben Assembly über **zwei getrennte Kestrel-Ports** betrieben werden.
* Jeder Endpoint **MUSS** explizit einer Zone zugeordnet sein. Die Bindung an den Port **MUSS** über `HttpContext.Connection.LocalPort` geprüft werden (niemals allein über den HTTP-`Host`-Header).
* `WebApi`-Endpoints **MÜSSEN** standardmäßig authentifiziert sein (`RequireAuthorization()`). Öffentliche Endpoints **MÜSSEN** explizit mit `AllowAnonymous()` markiert werden.
* **Streaming statt RAM-Buffering:** Audio-Dateien, Firmware-Images und größere Binärdaten **DÜRFEN NICHT** komplett als `byte[]` in den Arbeitsspeicher geladen werden; es **MUSS** gestreamt werden (`Stream` / `Results.Stream`) mit expliziten Größenlimits.
* **Hardware-/Flash-Zugriffe:** Zugriffe auf lokale Hardware/CLI-Tools (Flashen, Zertifikat-Extraktion) **MÜSSEN** in dedizierten Infrastruktur-Adaptern gekapselt werden – niemals direkt im Endpoint-Handler.
* **Echtzeit-Status:** Live-Fortschritte (z. B. Flashen, Konvertierung) **SOLLTEN** über Server-Sent Events (SSE) bereitgestellt werden.

### 2.6 Observability (Serilog, OpenTelemetry, Health)
* **Serilog:** **MUSS** im C#-Code anhand AOT-gebundener `LoggingOptions` konfiguriert werden (kein `Serilog.Settings.Configuration` über Reflection).
* Logs **MÜSSEN** strukturiert sein (Message Templates wie `LogInformation("Order {OrderId} created", orderId)` – **KEINE** String-Interpolation). Sensible Daten (Passwörter, Tokens, Zertifikate, PII) **DÜRFEN NICHT** geloggt werden.
* **Metrics & Traces:** Fachliche Metriken **MÜSSEN** über `System.Diagnostics.Metrics.Meter` und `ActivitySource` (OpenTelemetry) erfasst werden. Hochkardinale Werte (z. B. `UserId`, `RequestId`) **DÜRFEN NICHT** als Metrik-Tags verwendet werden.
* **Health-Checks:** Jede kritische Infrastruktur-Abhängigkeit (z. B. SQLite) **MUSS** über `/health` (`AddHealthChecks()`) geprüft werden.

---

## 3. API-Contract (Backend ↔ Frontend)

* Das Backend **MUSS** eine OpenAPI-Spezifikation generieren.
* Der TypeScript-API-Client und die DTO-Typen im Frontend **MÜSSEN** daraus generiert werden; manuelles Duplizieren von Backend-DTOs im Frontend **DARF NICHT** stattfinden.

---

## 4. Frontend-Richtlinien (React, Vite, TypeScript)

### 4.1 Vertical Slice Architecture im Frontend
* Feature-Code **MUSS** unter `src/features/<feature-name>/` gebündelt werden (API-Hooks, Feature-Komponenten, lokale Typen, Tests).
* Ein Feature **DARF NICHT** direkt in interne Dateien eines anderen Features importieren. Kommunikation läuft ausschließlich über definierte öffentliche Exports (`index.ts`), Router/URL oder globale Events.

### 4.2 Striktes UI-Modul (`src/ui/`) & HTML-Verbot
* **Kein rohes HTML in Features:** Rohe HTML-Tags (`<button>`, `<input>`, `<a>`, `<h1>`–`<p>`, `<div>`, `<span>` etc.) **DÜRFEN** ausschließlich innerhalb von `src/ui/` verwendet werden. Feature-Slices **MÜSSEN** UI- und Layout-Komponenten aus `src/ui/` zusammensetzen (z. B. `<Button>`, `<Stack>`, `<Card>`, `<Text>`).
* **Discovery-Pflicht:** Vor dem Erstellen einer neuen UI-Komponente **MUSS** geprüft werden, ob bereits eine passende Komponente in `src/ui/` existiert.
* **Erweiterungsregel:** Ist eine bestehende Komponente fast passend, **SOLLTE** sie abwärtskompatibel über Props (`variant`, `size`, `slot`) erweitert werden. Änderungen, die bestehende Verwendungen beeinträchtigen, **DÜRFEN NICHT** ohne Begründung, Doku-Update und explizite Genehmigung durchgeführt werden.
* **Dokumentationspflicht:** Jede UI-Komponente in `src/ui/` **MUSS** per TSDoc/JSDoc (Zweck, Props, Varianten) dokumentiert sein.
* **Zentrale Accessibility (a11y):** Tastaturnavigation, Fokus-Management, Kontrast und ARIA-Attribute **MÜSSEN** zentral in den Komponenten unter `src/ui/` gelöst werden.

### 4.3 CSS Modules & Theming (Design Tokens)
* Styling **MUSS** über **CSS Modules** (`*.module.css`) ausschließlich innerhalb von `src/ui/` erfolgen. Feature-Slices **DÜRFEN KEINE** eigenen CSS-Dateien oder Inline-Farben verwenden.
* **Keine hardcodierten Farben/Werte:** Farben, Abstände, Radien und Schatten **MÜSSEN** über semantische CSS Custom Properties aus der zentralen Theme-Definition (`src/ui/theme/tokens.css`, z. B. `--color-bg-surface`, `--color-action-primary`, `--space-md`) referenziert werden.
* Token-Namen **MÜSSEN** semantisch sein (z. B. `--color-action-primary`, nicht `--blue-500`), damit Themes (z. B. über `[data-theme="dark"]`) ohne Komponenten-Anpassung vollständig austauschbar sind.

### 4.4 Data Fetching & State Management
* Backend-Daten (Server State) **MÜSSEN** über **TanStack Query** (`useQuery`, `useMutation`) in Kombination mit dem generierten OpenAPI-Client verwaltet werden.
* Lokaler UI-Zustand (Client State) **SOLLTE** über `useState` / `useReducer` (und bei echtem globalem Bedarf über React Context, z. B. Theme/Auth) abgebildet werden.

---

## 5. Testing & Coverage

* **Backend Unit Tests:**
  * Handler, Validatoren, Domain-Logik und Mapper-Extensions **MÜSSEN** **100 % Branch Coverage** erreichen.
  * Reiner Startup-/Wiring-Code (`Program.cs`) sowie Source-Generator-Code sind ausgenommen (`[ExcludeFromCodeCoverage]`).
* **Backend Integration Tests:**
  * Jeder Slice **MUSS** über mindestens einen Integrationstest (z. B. mit `WebApplicationFactory` gegen eine isolierte SQLite-Testdatenbank) inkl. Port-Zonen- und Auth-Prüfung abgesichert sein.
* **Frontend Tests:**
  * Jede Komponente in `src/ui/` **MUSS** Component Tests (inkl. Interaktion und a11y-Basisprüfungen) besitzen.
  * Jedes Feature unter `src/features/` **MUSS** Feature Tests besitzen.

---

## 6. Automatisierte Durchsetzung (Linter & CI)

Alle verbindlichen Architekturregeln **SOLLTEN** automatisiert im Build/CI geprüft werden:
* **ESLint:** Verbot roher HTML-Tags außerhalb von `src/ui/`, Verbot direkter Cross-Feature-Imports (`boundaries`).
* **Stylelint:** Verbot hartkodierter Farb-Literale (`#...`, `rgb(...)`, `hsl(...)`) außerhalb der Theme-Token-Dateien.
* **Backend CI:** `dotnet publish` mit Native AOT sowie Coverage-Check gegen die 100 %-Branch-Schwelle.

---

## 7. Naming Conventions

* **Backend (C#):**
  * Slices: `Features/<Domain>/<ActionName>/` (z. B. `Features/Users/CreateUser/`).
  * Dateien pro Slice: `<Feature>Endpoint.cs`, `<Feature>Request.cs`, `<Feature>Response.cs`, `<Feature>Handler.cs`, `<Entity>MapperExtensions.cs`.
  * Testnamen: `<MethodName>_<Condition>_<ExpectedResult>` (z. B. `ToResponse_WhenUserHasMfa_SetsMfaEnabledTrue`).
* **Frontend (TypeScript / React):**
  * Feature-Ordner: `kebab-case` (z. B. `src/features/tonie-library/`).
  * UI-Komponenten & Dateien: `PascalCase` (`Button.tsx`, `Button.module.css`).
  * Hooks: `use<Name>.ts` (`useTonieList.ts`).
  * CSS Design Tokens: `--<kategorie>-<semantik>-<variante>` (z. B. `--color-bg-surface`, `--space-md`, `--radius-sm`).

---

## 8. Design-Prinzipien, Dateistruktur & Dateigröße

### 8.1 Design-Prinzipien (KISS, YAGNI, SOLID, DRY)
* **KISS & YAGNI:** Code **SOLLTE** einfach und direkt lesbar bleiben und auf spekulative Vorab-Abstraktionen verzichten (z. B. keine generischen Repositories über `Dapper.AOT`, keine vorab eingebauten Erweiterungs-Hooks ohne konkreten Bedarf).
* **SOLID (pragmatisch mit VSA & Native AOT):**
  * **SRP & ISP:** Jede Klasse, jeder Handler und jede React-Komponente hat genau eine klar abgegrenzte Verantwortung und hängt nur von schmalen Schnittstellen ab.
  * **OCP:** Neue Features werden über neue Vertical Slices bzw. Feature-Ordner ergänzt, ohne bestehende Slices umzubauen.
  * **DIP:** Infrastruktur-Abhängigkeiten (`ISqliteConnectionFactory`, `ILoggerFactory`, `IOptions<T>`) werden injiziert; zustandslose Pure Functions (Validatoren, Mapper, Crypto-Helper) bleiben AOT-freundlich `static`.
* **DRY mit Augenmaß:** Gemeinsame Infrastruktur, generierte API-Contracts und `src/ui/`-Komponenten **MÜSSEN** wiederverwendet werden. Zwischen unabhängigen Vertical Slices wird jedoch keine künstliche Vererbung oder gemeinsame Basisklasse erzwungen.

### 8.2 Dateistruktur (1 Haupttyp pro Datei)
* **Backend (C#):** Pro Datei **SOLLTE** genau **1 Haupttyp** (Klasse, Interface, Enum, Handler, Endpoint, Validator, Service) stehen (`Dateiname = Typname`).
  * *VSA-Ausnahme:* Sehr kleine, eng zum selben Slice gehörende Hilfs-Records (z. B. ein slice-internes DB-Projection-`record` beim Handler oder ein kleines `Request`/`Response`-Paar) dürfen zusammen liegen. Geteilte Typen, Interfaces und Implementierungen stehen immer in eigenen Dateien.
* **Frontend (TypeScript / React):** Pro Datei **SOLLTE** genau **1 Komponente** bzw. **1 Context/Hook-Modul** stehen (inkl. der direkt zugehörigen `Props`-/`Value`-Interfaces in derselben Datei).

### 8.3 Maximale Dateigröße
* Produktive Quelldateien **SOLLTEN** maximal **300 Zeilen** umfassen (Signal für einen SRP-Split) und **DÜRFEN** **500 Zeilen** nicht überschreiten.
* **Ausnahmen:** Automatisch generierte Dateien (`src/frontend/src/api/schema.d.ts`, Source-Generator-Output, Lockfiles) sowie reine Token- und SQL-Migrationsdateien. Testdateien **SOLLTEN** ebenfalls unter 500 Zeilen bleiben (bei Bedarf nach Szenario aufteilen).
