# Bestandteile des Repositorys

Das Repository enthaelt die eigenstaendige Zusatzmod LethalPhones Call Timeout.
Sie aendert die Wartezeit unbeantworteter Spieler-/Switchboard-Anrufe. Die
eigentlichen Klingeltoene und das private Medienwerkzeug sind getrennt davon.

## Die vier Startdateien

| Datei | Aufgabe |
| --- | --- |
| Build.cmd | Kompiliert den Quellcode zur Mod-DLL. Installiert nichts ins Spiel. |
| Test.cmd | Baut die DLL und fuehrt Logiktests, die Pruefung der installierten LethalPhones-Version und Unity-Laufzeittests aus. |
| Install-Test.cmd | Baut und testet, fragt den Timer-Modus ab und installiert im Profil aus local.settings.json. Sichert vorhandene Dateien und deaktiviert den alten privaten Timerfix. Das Spiel muss geschlossen sein. |
| Release.cmd | Baut und testet und erstellt das Thunderstore-Uploadpaket. Verlangt dein ausgefuelltes README. Fuehrt keinen Upload aus. |

Die CMD-Dateien rufen die PowerShell-Skripte in scripts/ auf. Sie aendern weder
die globale PowerShell-Richtlinie noch installieren sie Software global.

## Quellcode in src/

| Datei | Aufgabe |
| --- | --- |
| Plugin.cs | Einstiegspunkt fuer BepInEx, Mod-ID und Version, Konfiguration und Aktivierung der Patches. |
| TimeoutPolicy.cs | Berechnet die Wartezeit: fester Wert oder Tonlaenge, Ersatzwert, Obergrenze und optionaler Zusatzpuffer. |
| LethalPhonesRuntime.cs | Findet das angerufene Telefon und dessen Klingelton in LethalPhones; liest dessen tatsaechliche AudioClip-Laenge. |
| CallTimeoutPatches.cs | Veraendert die entsprechenden LethalPhones-Timer ueber Harmony und fuehrt die Geraeuschmeldungen waehrend laengerer Klingelzeiten fort. |
| AssemblyInfo.cs | Technische DLL-Metadaten einschliesslich Assembly-/Dateiversion. |

Die normale LethalPhones-DLL wird dabei nicht veraendert oder mitgeliefert.

## Einstellungen und Entwicklungsdateien

| Datei | Aufgabe |
| --- | --- |
| local.settings.json | Deine lokalen Spiel-, Profil- und Unity-Editor-Pfade. Ist im bereitgestellten ZIP vorausgefuellt, wird aber von Git ignoriert und nicht ins Uploadpaket gepackt. |
| local.settings.example.json | Oeffentliche Vorlage ohne deine konkreten Pfade. Wer das Repository spaeter klont, erstellt daraus seine eigene local.settings.json. |
| LethalPhones.CallTimeout.csproj | Projektdatei fuer eine Entwicklungsumgebung mit vorhandenem .NET-SDK. Die CMD-Dateien koennen ohne zusaetzliche SDK-Installation bauen. |
| .git/ | Lokale Git-Verwaltung. Branch main ist vorbereitet; es gibt noch keinen ersten Commit und keinen Remote. Die *.sample-Dateien in .git/hooks sind inaktive Git-Vorlagen. |
| .gitignore | Schliesst lokale Pfade, Buildausgaben, Sicherungen und temporaere Unity-Dateien von Git aus. |
| .gitattributes | Regelt Zeilenenden und kennzeichnet Binaerdateien fuer Git. |
| LICENSE.txt | Mitgelieferte MIT-Lizenz. In der Copyright-Zeile steht bisher nur das Jahr; hier kannst du deinen Namen oder Alias ergaenzen. |

local.settings.json steuert die Entwicklung auf deinem Rechner. Die
Spieleinstellungen stehen dagegen nach Installation/erstem Start im Profil
unter BepInEx/config/lethalphones.calltimeout.cfg. Diese Datei kannst du auch
ueber den Config Editor von r2modman bearbeiten. Details: KONFIGURATION.md.

## Oeffentliche Modbeschreibung und Paketdaten

| Datei | Aufgabe |
| --- | --- |
| README.md | Dein oeffentlicher Beschreibungstext fuer GitHub und Thunderstore. Den Platzhalter WRITE_YOUR_README vor Release.cmd ersetzen. |
| package/manifest.json | Modname, Version, Kurzbeschreibung, optionale Repository-URL und Abhaengigkeiten. Team und Kategorien werden beim Web-Upload ausgewaehlt. |
| package/icon.png | Das mitgelieferte 256 x 256 Pixel grosse Thunderstore-Symbol; kann durch dein eigenes PNG dieser Groesse ersetzt werden. |
| package/CHANGELOG.md | Oeffentliche Liste der Aenderungen pro Modversion. |

Release.cmd setzt diese Dateien an die richtigen Stellen im Upload-ZIP. Du
musst sie nicht von Hand umsortieren. docs/ wird nicht ins Mod-Uploadpaket
gepackt; Links auf diese Anleitung im oeffentlichen README daher als komplette
GitHub-URLs eintragen, sobald dein Repository dort existiert.

## Skripte in scripts/

| Datei | Aufgabe |
| --- | --- |
| Build.ps1 | Steuert Bauen, Testen, Testinstallation und Release. Verwendet die bereits vorhandenen lokalen Werkzeuge und Referenzen. |
| Verify-References.ps1 | Prueft die vorhandene LethalPhones-DLL auf die erwartete Struktur und die zu ersetzenden 15-Sekunden-Timer. |
| Install-Test.ps1 | Installiert ausschliesslich die Zusatzmod und deren Konfiguration im gewaehlten Profil; erstellt Sicherungen fuer Updates. |
| Package.ps1 | Prueft Modversion und DLL-Testnachweis und erzeugt das fertige Thunderstore-Uploadpaket. |

## Tests und Berichte

| Datei/Ordner | Aufgabe |
| --- | --- |
| tests/CoreTests.cs | 20 Tests der reinen Timerberechnung, ohne das Spiel zu starten. |
| tests/UnityHarness/Assets/Editor/CallTimeoutRuntimeTests.cs | 24 Laufzeittests mit echten Harmony-Patches und AudioClips im vorhandenen Unity-Editor, mit isolierten Telefon-Testklassen. |
| tests/UnityHarness/Packages/manifest.json | Paketliste des isolierten Unity-Testprojekts; hat nichts mit dem Thunderstore-Manifest zu tun. |
| tests/UnityHarness/ProjectSettings/ProjectVersion.txt | Versionsangabe des Unity-Testprojekts. |
| docs/START-HIER.md | Einstieg und Voraussetzungen. |
| docs/KONFIGURATION.md | Timer-Modi, Optionen und Mehrspieler-Verhalten. |
| docs/TESTEN.md | Automatische Tests, Spielprobe fuer spaetere Aenderungen und Rueckbau. |
| docs/TESTERGEBNIS.md | Bestehende Testergebnisse einschliesslich des vom Nutzer bestaetigten Lobbytests. |
| docs/REPO-UEBERSICHT.md | Diese Dateierklaerung. |
| docs/VEROEFFENTLICHEN.md | Konten, Team, Kategorien und manueller Upload. |

## Erzeugte Dateien in artifacts/

| Ordner | Inhalt |
| --- | --- |
| artifacts/bin/ | Fertige LethalPhones.CallTimeout.dll. Die bereits gepruefte DLL ist im bereitgestellten Repo-ZIP enthalten. |
| artifacts/tests/ | Testprogramme, Protokolle und verification.json mit dem SHA256-Nachweis fuer die automatisch getestete DLL. Der Lobbytest steht getrennt in TESTERGEBNIS.md. |
| artifacts/releases/ | Entsteht durch Release.cmd. Hier liegt die fuer Thunderstore bestimmte ZIP, derzeit LethalPhones_CallTimeout-1.0.0.zip. |
| artifacts/backups/ | Entsteht bei einer Testinstallation, wenn vorhandene Dateien gesichert werden muessen. |

Der ganze artifacts/-Ordner bleibt aus Git ausgeschlossen. Die DLL und der
automatische Testnachweis werden nur fuer dein bequemes Startpaket explizit
im Repo-ZIP mitgeliefert. Das oeffentliche Modpaket enthaelt die DLL, den
Modquellcode und die oeffentlichen Paketdateien; keine lokalen Pfade, Bilder,
Klingeltoene, Profilarchive oder Sicherungen.
