# Veroeffentlichen: Konten, Kategorien und Dateien

Stand: 23.10.2026. Der Nutzer hat den Lobbytest der bereitgestellten Fassung
bestaetigt. Es wird nichts automatisch veroeffentlicht.

## Welche Konten brauchst du?

- Git auf deinem Rechner: kein Online-Konto erforderlich. Das Repository ist
  vorbereitet, aber noch ohne Commit und ohne Verbindung zu einem Server.
- GitHub: ein Konto nur dann, wenn du das Repository dort hochladen moechtest.
  Fuer den manuellen Thunderstore-Upload ist ein GitHub-Repository nicht erforderlich.
- Thunderstore: zum Veroeffentlichen anmelden bzw. einen Account anlegen.
  Waehle beim Upload ein Team; falls du noch keines hast, lege eines an.
  Du kannst das einzige Mitglied dieses Teams sein. Der Teamname ist der
  oeffentliche Herausgeber/Namespace deiner Mod.
- Mitspieler brauchen kein Herausgeber-Konto, um die veroeffentlichte Mod
  ueber r2modman zu installieren.

Thunderstore: https://thunderstore.io/
Teams verwalten/anlegen: https://thunderstore.io/teams/
Manueller Upload: https://thunderstore.io/package/create/
Optionales Quellcode-Repository anlegen: https://github.com/new

Es werden weder ein API-Token noch ein Service-Account fuer den manuellen
Upload benoetigt. Das Projekt enthaelt keinen automatischen Upload-Workflow.

## Was bearbeitest du wo?

| Angabe | Ort |
| --- | --- |
| Ausfuehrliche oeffentliche Beschreibung | README.md im Repo-Hauptordner |
| Oeffentlicher Modname | package/manifest.json, Feld name; derzeit LethalPhones_CallTimeout |
| Kurzbeschreibung | package/manifest.json, Feld description; maximal 250 Zeichen |
| Modversion | package/manifest.json, Feld version_number; derzeit 1.0.0 |
| GitHub-/Website-Link | package/manifest.json, Feld website_url; leer lassen oder spaeter deine echte Repository-URL eintragen |
| Benoetigte Mods | package/manifest.json, Feld dependencies; LethalPhones und BepInEx sind bereits eingetragen |
| Vorschaubild | package/icon.png; PNG mit genau 256 x 256 Pixeln |
| Versionsaenderungen | package/CHANGELOG.md |
| Oeffentlicher Herausgeber | Team-Auswahl im Thunderstore-Uploadformular |
| Spiel | Communities im Uploadformular: Lethal Company |
| Tags/Kategorien | Categories im Uploadformular, nach Auswahl der Community |

Die Modversion steht ausserdem im BepInPlugin-Attribut in src/Plugin.cs sowie
als AssemblyVersion und AssemblyFileVersion in src/AssemblyInfo.cs. Bei einem
spaeteren Versionswechsel diese Angaben gemeinsam mit dem Manifest erhoehen.
Fuer die erste Veroeffentlichung kannst du alles auf 1.0.0 belassen.

## Welche Tags/Kategorien?

Fuer dieses Paket passen Mods, BepInEx und Tweaks & Quality Of Life.
Audio ist als weitere Kategorie passend. Es ist die Timer-Zusatzmod, kein
Modpack und kein Paket mit deinen privaten Audiodateien.

Waehle zusaetzlich AI Generated: Thunderstore verlangt diese Kennzeichnung,
wenn ein wesentlicher Teil mit KI-Werkzeugen erstellt wurde. Das trifft auf
den hier erstellten Modcode zu. Der entsprechende Hinweis steht auf der
offiziellen Legacy-Seite von Lethal Company. Die Kategorie wird im Webformular
gesetzt und nicht in package/manifest.json eingetragen.

Die Mod soll auf allen Clients installiert sein, damit beide Anrufrichtungen
angepasst werden. Dies im eigenen README angeben; eine Kategorie allein
erklaert die Installationsanforderung nicht.

## Sind Git-Tags noetig?

Nein. Ein Git-Tag wie v1.0.0 markiert optional einen bestimmten Commit als
Version. Ein GitHub-Release kann daran haengen und beispielsweise die fertige
Mod-ZIP als Download anbieten. Fuer den manuellen Thunderstore-Upload ist
beides nicht erforderlich. Thunderstore liest die Versionsnummer aus dem
manifest.json im Upload-ZIP. Git-Tags, GitHub-Themen und Thunderstore-Kategorien
sind voneinander unabhaengig.

## Dein Ablauf fuer die erste Veroeffentlichung

1. Deinen README-Platzhalter ersetzen. Optional Icon, Kurzbeschreibung,
   Copyright-Name/Alias und Repository-URL anpassen.
2. Wenn gewuenscht das GitHub-Repository anlegen, den ersten lokalen Commit
   mit deiner Identitaet erstellen und das Repository hochladen. Noch ist kein
   Remote gesetzt; es wird nichts automatisch gepusht.
3. Release.cmd starten. Die Datei artifacts/releases/LethalPhones_CallTimeout-1.0.0.zip
   wird gebaut. Dein README muss ausgefuellt sein; die automatischen Tests
   muessen fuer die gerade erzeugte DLL bestehen.
4. Auf Thunderstore anmelden. Das Uploadformular oeffnen und diese Release-ZIP
   auswaehlen. Team auswaehlen bzw. anlegen, Community Lethal Company und die
   oben genannten Kategorien auswaehlen. Dann selbst auf Submit klicken.
5. Die veroeffentlichte Mod ueber r2modman ins Profil installieren. Danach das
   Profil fuer die Mitspieler exportieren. Die Mod kann dann als normale
   Thunderstore-Abhaengigkeit beim Profilimport mitkommen.

Fuer Thunderstore ist die ZIP unter artifacts/releases/ bestimmt. Das separate
LethalPhonesCallTimeout-GitRepo.zip ist dein Entwicklungsprojekt und nicht das
Uploadpaket. Der Medienexport mit privaten Bildern/Klingeltoenen bleibt davon
unabhaengig und kommt weiterhin ueber dein privates Profilarchiv.

Spaetere Updates: Team und Modname beibehalten, Version erhoehen und eine neue
Release-ZIP hochladen. Eine bereits veroeffentlichte Version ist unveraenderlich;
auch fuer spaetere README-Aenderungen ist eine neue Paketversion erforderlich.

## Primaerquellen

- Uploadformular mit Team, Community und Categories: https://thunderstore.io/package/create/
- Lethal-Company-Kategorien: https://thunderstore.io/c/lethal-company/
- Offizieller Hinweis zur KI-Kennzeichnung: https://old.thunderstore.io/c/lethal-company/
- Paketformat und Manifest: https://wiki.thunderstore.io/mods/creating-a-package
- Updates und unveraenderliche Versionen: https://wiki.thunderstore.io/mods/updating-a-package
