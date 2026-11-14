# Projekt- und Coding-Richtlinien (WunderHorst)

> Schlüsselwörter: **MUSS / DARF NICHT** = verbindlich, **SOLLTE** = Standard, Abweichung nur mit Begründung.

---

## 💻 Coding Guidelines (Verbindlich)

- **Pflichtlektüre vor jeder Implementierung:** Sämtliche technischen Vorgaben für Backend und Frontend stehen in [`docs/development/coding-guidelines.md`](docs/development/coding-guidelines.md) und **MÜSSEN** strikt eingehalten werden.
- **Architektur:** Backend (.NET 10 Minimal APIs, Native AOT, SQLite + Dapper.AOT) und Frontend (React + Vite + TypeScript, `pnpm`, CSS Modules + Design Tokens, TanStack Query) **MÜSSEN** nach **Vertical Slice Architecture** umgesetzt werden.
- **Native AOT & Mapper:** Keine Reflection im Backend (`System.Text.Json` Source Generation, AOT-Configuration-Binding). Mapper **MÜSSEN** als Extension Methods geschrieben werden.
- **Port-Zonen & Sicherheit:** `WebApi` und `BoxineApi` **MÜSSEN** auf getrennten Kestrel-Ports laufen (Prüfung via `Connection.LocalPort`). Große Binärdaten **MÜSSEN** gestreamt werden.
- **UI-Modul & Theming:** Rohe HTML-Tags und CSS Modules (`*.module.css`) **DÜRFEN** nur in `src/ui/` verwendet werden. Vor neuen UI-Komponenten **MUSS** geprüft werden, ob bestehende erweitert werden können (ohne Breaking Changes ohne Genehmigung). Farben/Werte **DÜRFEN NICHT** hardcodiert werden – nur semantische CSS-Variablen.
- **Testing & Observability:** Backend Handler/Validatoren/Mapper **MÜSSEN** 100 % Branch Coverage erreichen (+ Integrationstests pro Slice); Frontend braucht Component- & Feature-Tests. Logs **MÜSSEN** strukturiert über Serilog erfolgen; Metrics/Traces über OpenTelemetry; Abhängigkeiten in `/health`.

---

## 📦 Git- & Commit-Richtlinien (Wichtig)

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
