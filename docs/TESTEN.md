# Tests und spaetere Funktionspruefungen

Stand 23.10.2026: Der Nutzer hat den Lobbytest der bereitgestellten Fassung 1.0.0
als erfolgreich bestaetigt. Die folgende Spielprobe bleibt als Anleitung fuer
spaetere Aenderungen erhalten; sie ist keine noch offene Veroeffentlichungshuerde.

## Automatische Tests

Test.cmd kompiliert die DLL und prueft getrennt:

1. Reine Timerlogik: feste Dauer, individuelle Laengen, Ersatzwerte, Grenzen,
   ungueltige Werte und Zusatzpuffer.
2. Die tatsaechlichen Metadaten und Zielmethoden der lokal installierten
   LethalPhones-Version.
3. Echte Harmony-Aenderungen in Unity mit isolierten Telefon-Testklassen:
   individuelle Empfaenger-Toene, Switchboard, fehlende Assets, normale
   Beendigung, Abbruch und erhaltene Monster-Timer.

Der Unity-Test startet den vorhandenen Editor ohne interaktives Fenster und
schliesst ihn anschliessend. Temporaere Unity-Dateien bleiben unter tests/ und
werden weder von Git erfasst noch in das Repo-ZIP oder den Mod-Release gepackt.

## Echte Spielprobe bei spaeteren Aenderungen

1. Spiel schliessen und Install-Test.cmd starten. Den gewuenschten Modus waehlen.
2. Spiel wie gewohnt ueber r2modman starten. Im Log nach
   'Call timeout patches active' suchen. Es darf kein Patchfehler erscheinen.
3. Im Modus Fixed zunaechst 45 Sekunden ohne Annahme klingeln lassen.
4. Einen eingehenden Anruf annehmen und spaeter manuell auflegen.
5. Bei einem weiteren Anruf waehrend des Klingelns manuell auflegen.
6. Im Modus RingtoneLength zwei gut unterscheidbare Tonlaengen pruefen,
   beispielsweise ungefaehr 17 und 40 Sekunden. Vom gepatchten Client aus anrufen.
7. Die andere Anrufrichtung mit ebenfalls installiertem Fix pruefen.
8. Einen Anruf vom und zum Switchboard pruefen.

Die Test-DLL muss fuer beide Anrufrichtungen auf beiden Rechnern installiert
sein. Der Test mit nur einem gepatchten Anrufer prueft nur diese eine Richtung.
Dieser lokale Testschritt ist fuer die Entwicklung. Nach einer normalen
Thunderstore-Veroeffentlichung wird die Mod ueber das Profil mitinstalliert.

## Rueckgaengig machen

Spiel schliessen und BepInEx/plugins/LethalPhonesCallTimeout entfernen.
Der Testinstaller legt vor Updates Sicherungen unter artifacts/backups an.
Ein zuvor deaktivierter privater Fix bleibt als .dll.disabled erhalten und
kann bei Bedarf nach Entfernen der neuen Mod zurueckbenannt werden.

Das Medienwerkzeug, Bilder und Klingeltoene werden durch diese Tests nicht veraendert.
