# Eigenstaendiges Repository

Dieses Projekt ist nur die LethalPhones-Zusatzmod. Es enthaelt weder das
Medienwerkzeug noch Bilder, Klingeltoene, Profilarchive oder Download-Funktionen.

Das enthaltene Git-Repository kannst du samt .git-Ordner an einem beliebigen
Ort entpacken. Es hat den Branch main, keinen Remote und keine automatischen
Commits oder Uploads. Du kannst nach deinem README selbst den ersten Commit
mit deiner eigenen Git-Identitaet erstellen.

## Bei dir ist schon alles vorbereitet

- Build.cmd: kompiliert die Zusatzmod mit dem vorhandenen Windows-C#-Compiler.
- Test.cmd: baut neu und fuehrt die automatischen Tests aus.
- Install-Test.cmd: installiert die Test-DLL ausschliesslich im ausgewaehlten
  Modprofil. Das Spiel muss geschlossen sein. Ein alter privater Timerfix wird
  gesichert und deaktiviert. Danach kannst du den Modus fuer den Test auswaehlen.
- Release.cmd: baut, testet und erstellt die Thunderstore-ZIP mit DEINEM README.
  Solange WRITE_YOUR_README noch im README steht, wird keine Release-ZIP erstellt.

Die fertige und getestete DLL liegt bereits unter artifacts/bin/.
Die ZIP fuer den Upload entsteht nach dem Bearbeiten des README unter
artifacts/releases/. Der Code nimmt keinen Upload und keine Git-Pushes vor.

## Lokale Pfade

local.settings.json ist fuer deine vorhandene Installation vorausgefuellt.
Die Datei wird von Git ignoriert und landet nicht in der Thunderstore-ZIP.
Nach dem Entpacken an einem anderen Ort bleiben die eingetragenen Spiel- und
Profilpfade gueltig. Wenn sich diese aendern, bearbeite local.settings.json.

Build und Tests installieren nichts global und benoetigen keine Downloads.
Der C#-Compiler kommt aus dem vorhandenen Windows .NET Framework. Ein zusaetzliches
dotnet-SDK oder NuGet-Restore ist fuer die mitgelieferten CMD-Dateien nicht noetig.
Die csproj-Datei ist zusaetzlich fuer IDEs/Entwickler mit vorhandenem SDK gedacht.

Fuer den Unity-Laufzeittest wird der schon vorhandene Unity-Editor verwendet.
Sein Pfad ist in local.settings.json eingetragen. Es wird kein Editor installiert.
Ohne vorhandenen Editor sind die reinen Logiktests moeglich; ein neues Release
erfordert aber einen erfolgreichen Unity-Test derselben DLL.

Die CMD-Dateien erlauben PowerShell-Skripte nur fuer ihren eigenen Prozess.
Die globale PowerShell-Ausfuehrungsrichtlinie wird nicht geaendert.

## Dein README und deine Veroeffentlichung

README.md ist bewusst nur ein Platzhalter. docs/KONFIGURATION.md beschreibt die
Funktionen; docs/TESTEN.md und docs/TESTERGEBNIS.md dokumentieren die Pruefung.
Du entscheidest ueber Formulierung, Git-Remote, Thunderstore-Team und Upload.
Optional kannst du deine Repository-URL in package/manifest.json eintragen.

Der Nutzer hat den Lobbytest der bereitgestellten Fassung am 23.10.2026 als
erfolgreich bestaetigt. Der Hinweis auf einen noch offenen Lobbytest ist entfernt.
Die Spielprobe in TESTEN.md bleibt als Anleitung fuer spaetere Aenderungen erhalten.

REPO-UEBERSICHT.md erklaert die einzelnen Dateien und Ordner.
VEROEFFENTLICHEN.md erklaert Konten, Team, Kategorien und den manuellen Upload.
