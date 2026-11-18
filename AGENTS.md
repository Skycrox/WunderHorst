# Projekt- und Coding-Richtlinien (WunderHorst)

> Schlüsselwörter: **MUSS / DARF NICHT** = verbindlich, **SOLLTE** = Standard, Abweichung nur mit Begründung.

---

## 🎯 Projektkontext

- **WunderHorst** ist ein modernes, eigenständiges Self-Hosted Drop-in-Replacement für die Toniebox (Boxine-Protokoll, Audio-/TAF-Pipeline, Tonie-Bibliothek und Box-Verwaltung).
- Ziel ist eine schlanke, performante und wartbare Architektur auf Basis von **.NET 10 (Native AOT)**, **SQLite + Dapper.AOT** und einem **React + Vite + TypeScript**-Frontend.

---

## 💻 Coding Guidelines (Verbindlich)

- **Pflichtlektüre vor jeder Implementierung:** Sämtliche technischen Vorgaben für Backend und Frontend stehen in [`docs/development/coding-guidelines.md`](docs/development/coding-guidelines.md) und **MÜSSEN** strikt eingehalten werden.
- **Architektur:** Backend (.NET 10 Minimal APIs, Native AOT, SQLite + Dapper.AOT) und Frontend (React + Vite + TypeScript, `pnpm`, CSS Modules + Design Tokens, TanStack Query) **MÜSSEN** nach **Vertical Slice Architecture** umgesetzt werden.
- **Shared-Code („Rule of Two“) & Modulgrenzen:** Gemeinsamer Code in `Shared/` bzw. globalen Frontend-Ordnern ist erst erlaubt, wenn mindestens **zwei unabhängige Features** ihn nachweislich benötigen (sonst bleibt er im jeweiligen Slice/Feature). Zirkuläre Abhängigkeiten und direkte Deep-Imports in fremde Features **DÜRFEN NICHT** entstehen.
- **Native AOT & Mapper:** Keine Reflection im Backend (`System.Text.Json` Source Generation, AOT-Configuration-Binding). Mapper **MÜSSEN** als Extension Methods geschrieben werden.
- **Datenbank & Migrationen:** Schema-Änderungen an SQLite **MÜSSEN** über versionierte SQL-Migrationen erfolgen und **zwingend im selben Commit** wie die zugehörige Code-Änderung enthalten sein.
- **Port-Zonen & Sicherheit:** `WebApi` und `BoxineApi` **MÜSSEN** auf getrennten Kestrel-Ports laufen (Prüfung via `Connection.LocalPort`). Große Binärdaten **MÜSSEN** gestreamt werden.
- **UI-Modul, Theming & Sprache:** Rohe HTML-Tags und CSS Modules (`*.module.css`) **DÜRFEN** nur in `src/ui/` verwendet werden. Vor neuen UI-Komponenten **MUSS** geprüft werden, ob bestehende erweitert werden können (ohne Breaking Changes ohne Genehmigung). Farben/Werte **DÜRFEN NICHT** hardcodiert werden – nur semantische CSS-Variablen. Native Browser-Dialoge (`alert()`, `confirm()`, `prompt()`) sowie Layout-verschiebende Inline-Banner **DÜRFEN NICHT** verwendet werden. Alle UI-Texte, `alt`-Attribute und nutzerseitigen Fehlermeldungen **MÜSSEN** auf Deutsch verfasst sein.
- **Testing & Observability:** Backend Handler/Validatoren/Mapper **MÜSSEN** 100 % Branch Coverage erreichen (+ Integrationstests pro Slice); Frontend braucht Component- & Feature-Tests. Jeder Bugfix **MUSS** einen Regressionstest enthalten (der ohne den Fix fehlschlägt); vor größeren Umbauten **MÜSSEN** Charakterisierungstests das bestehende Verhalten absichern. Logs **MÜSSEN** strukturiert über Serilog erfolgen; Metrics/Traces über OpenTelemetry; Abhängigkeiten in `/health`.
- **Design-Prinzipien & Dateigröße:** KISS, YAGNI, SOLID und DRY, **1 Haupttyp pro Datei** sowie die Dateigrößen-Grenze (**300 Zeilen Richtwert / 500 Zeilen Limit**) gelten gemäß [`coding-guidelines.md`](docs/development/coding-guidelines.md).

---

## 📦 Git- & Commit-Richtlinien (Wichtig)

- **Nur bei grünem Build & grünen Tests committen**: Commits sind **nur** zulässig, wenn Build, Linter und alle Tests erfolgreich durchlaufen.
- **Jede abgeschlossene Änderung zwingend committen**: Jede erfolgreich durchgeführte Aufgabe, jedes Feature, jeder Meilenstein und jede Änderung an Dokumentation oder Richtlinien (inkl. `AGENTS.md`, configs) **MUSS** abgeschlossen und direkt committet werden. Vor dem Beenden einer Antwort dürfen keine fertiggestellten Änderungen uncommittet in der Arbeitskopie verbleiben.
- Granular committen: keine Vermischung unabhängiger Features in einem Sammel-Commit.
- Prägnante Commit-Messages (bevorzugt Conventional Commits: `feat(...)`, `fix(...)`, `refactor(...)`, `docs(...)`, `chore(...)`).
- Vor dem Commit prüfen, dass keine Build-Artefakte oder temporären Dateien gestaged sind.

---

## 📚 Dokumentations-Richtlinien (`/docs/`) (Wichtig)

- Sämtliche technische Dokumentation liegt in `docs/`; Einstiegsseite ist [`docs/index.md`](docs/index.md) (listet alle Dokumente).
- **4-Säulen-Struktur**:
  - `docs/architecture/`: Systemarchitektur, Querschnittskonzepte (z. B. Schichtenmodell, Auth, Events, Datenbank).
  - `docs/features/`: Fachliche Features (z. B. `docs/features/<feature-name>.md`).
  - `docs/external-apis/`: Drittanbieter-APIs, Crawler, Datenquellen.
  - `docs/development/`: Entwickler-Guides, Setup, Testing, Deployment.
- **Unterordner-Regel**: Brauchen Themen mehrere Dokumente/Grafiken, wird ein Ordner mit **exakt dem Namen des Themas** angelegt (z. B. `docs/features/<feature-name>/index.md`).
- **Neues Feature ⇒ Doku Pflicht**: `docs/features/<feature-name>.md` mit Zielsetzung, Systemkomponenten, Datenmodell/Events, Endpunkten, Beispielen; in `docs/index.md` verlinken.
- **Konsultationspflicht**: Vor neuen Features, externen Anbindungen oder Änderungen an bestehenden Komponenten **zuerst** in `docs/` nach relevanter Doku suchen.
- **Aktualisierungspflicht**: Bei neuen externen APIs, geänderten Schnittstellen oder neuen Architekturmustern Doku pflegen und in `docs/index.md` verlinken.

---

## ✅ Definition of Done (vor jeder „Fertig“-Meldung abarbeiten)

- [ ] Nur das umgesetzt, was ausdrücklich gewünscht war (keine ungefragten Zusatz-Features)
- [ ] VSA, 1 Haupttyp pro Datei (mit VSA-Ausnahme) und Dateigröße (≤ 300 Richtwert / ≤ 500 Limit) eingehalten
- [ ] Shared-Code-Regel („Rule of Two“) sowie keine Deep-Imports oder zirkulären Abhängigkeiten zwischen Features
- [ ] `src/ui/` geprüft und wiederverwendet (keine rohen HTML-Tags in Features, keine hardcodierten Farben, keine nativen Browser-Dialoge, UI-Texte auf Deutsch)
- [ ] Tests vorhanden (100 % Branch Coverage auf Backend-Handlern/Validatoren/Mappern, Integrationstests, Frontend-Tests, Regressionstest bei Bugfix)
- [ ] Backend: `dotnet build` und `dotnet test` grün (bei AOT-/Release-relevanten Änderungen zusätzlich `dotnet publish`)
- [ ] Frontend (`src/frontend/`): `pnpm lint`, `pnpm test` und `pnpm build` grün
- [ ] Datenbankschema geändert ⇒ versionierte SQL-Migration im selben Commit
- [ ] Doku in `docs/` aktualisiert und in `docs/index.md` verlinkt (bei neuem Feature / geänderter Schnittstelle)
- [ ] Änderungen sauber und granular committet (keine offenen Änderungen in der Arbeitskopie)
