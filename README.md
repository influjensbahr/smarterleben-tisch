KielRegion TUIO Prototype

Deutsch

Einleitung, Beschreibung, Kontext
- Interaktive Anwendung für Multi‑Touch/Tangible‑Interaktionen auf Basis von TUIO 1.1 in Unity.
- Visualisiert Projekte der KielRegion nach Kategorien und zeigt Detailinformationen an.
- Ziel: Nachnutzbarer Showroom/Exponat-Baustein für Kommunen.

Funktionsumfang
- TUIO‑Anbindung (UDP/WebSocket) inkl. Cursor-, Object- und Blob‑Events.
- Visualisierung und Interaktion: Objekt‑Erkennung, Ring-/Listen‑Anordnung, Detailansicht.
- Datenquelle: CSV aus StreamingAssets inkl. Bildablage.
- Exporter (Editor): ScriptableObjects -> CSV + Bildexport.
- Idle‑/Start‑Animationen, UI‑Hilfskomponenten (Aspect Ratio, Button Toggle etc.).

Installation und Betrieb
1) Unity öffnen (empfohlen: LTS, z. B. 6000.2.10f1) und Projekt laden.
2) Abhängigkeiten prüfen: DOTween, TuioNet/TuioUnity, TextMeshPro.
3) StreamingAssets: projects.csv und Bilderordner KielRegionImages bereitstellen.
4) TUIO‑Quelle konfigurieren: Assets/_scripts/tuio/CustomTuioSessionBehaviour (IP/Port bzw. Dateien ip_address.txt, port.txt in StreamingAssets).
5) Szene starten, TUIO‑Sender verbinden.

Aktualisierung und Entwicklerdokumentation
- Code‑Stil: einheitliche Benennung, XML‑Summaries, Null‑Guards, Fehlerbehandlung (vgl. Code).
- Contribution Guidelines: Fork/Branch, PR mit Beschreibung, CI/Checks, Review erforderlich.
- Editor‑Tools: CSV‑Exporter unter Menü „KielRegion/Export CSV from ScriptableObjects“.

Nutzerdokumentation
- Objekte platzieren/erkennen: passende Marker/Symbol‑IDs verwenden.
- Tippen/Touch: Projektliste oder Details anzeigen; Zurückblendung per UI.
- Status: TUIOConnectionChecker zeigt Verbindungsstatus an.

Weitere Dokumentation
- publiccode.yml im Root zur Aufnahme in OpenCoDE.
- SBOM (CycloneDX) als SBOM.cyclonedx.json.

Code‑Dokumentation
- Kernklassen in Assets/_scripts mit Zusammenfassungen und Kommentaren versehen.

Lizenzhinweis
- Standardlizenz: EUPL 1.2. Siehe LICENSE.txt im Root.
- Drittanbieter‑Komponenten siehe SBOM und ggf. Lizenzhinweise der jeweiligen Pakete.

English

Introduction and Context
- Interactive prototype for multi‑touch/tangible interactions based on TUIO 1.1 in Unity.
- Visualizes KielRegion projects by category and shows details.
- Goal: Reusable showroom/exhibit building block for municipalities.

Features
- TUIO integration (UDP/WebSocket) with cursor, object and blob events.
- Visualization & interaction: object recognition, ring/list arrangement, details view.
- Data source: CSV from StreamingAssets including images.
- Exporter (Editor): ScriptableObjects -> CSV + image export.
- Idle/start animations and UI helpers (aspect ratio, button toggle, etc.).

Installation and Operation
1) Open in Unity (recommended LTS, e.g. 6000.2.10f1).
2) Ensure dependencies: DOTween, TuioNet/TuioUnity, TextMeshPro, optionally LeTai TrueShadow.
3) Provide StreamingAssets with projects.csv and images folder KielRegionImages.
4) Configure TUIO source: Assets/_scripts/tuio/CustomTuioSessionBehaviour (IP/port or ip_address.txt, port.txt in StreamingAssets).
5) Run the scene and connect your TUIO sender.

Updates and Developer Docs
- Code style: consistent naming, XML summaries, null‑guards, error handling (see code).
- Contribution guidelines: Fork/branch, PR with description, CI/checks, code review required.
- Editor tools: CSV exporter under menu “KielRegion/Export CSV from ScriptableObjects”.

User Documentation
- Place/recognize physical objects: use appropriate markers/symbol IDs.
- Touch interaction shows project list or details; close via UI controls.
- Status: TUIOConnectionChecker displays connection status.

Further Documentation
- publiccode.yml in the project root for OpenCoDE.
- SBOM (CycloneDX) as SBOM.cyclonedx.json.

Code Documentation
- Core scripts under Assets/_scripts contain summaries and comments.

License Notice
- Default license: EUPL 1.2. See LICENSE.txt in the repository root.
- Third‑party components: see SBOM and respective package licenses.

