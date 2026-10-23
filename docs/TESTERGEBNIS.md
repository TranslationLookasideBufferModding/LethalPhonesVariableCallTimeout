# Pruefstand 23.10.2026

## Automatisch bestanden

- 20 reine Logiktests fuer feste Dauer, individuelle Laenge, Ersatzwerte,
  Zusatzpuffer, Ober-/Untergrenzen und ungueltige Werte.
- Referenzpruefung an der vorhandenen LethalPhones-1.3.23-DLL: Plugin-ID,
  Coroutine-Zielfelder, die beiden bisherigen 15-Sekunden-Timer und die benoetigten
  Telefon-/Klingeltonfelder vorhanden.
- 24 Unity-Laufzeittests mit den echten Harmony-Patches und isolierten
  LethalPhones-Testklassen. Darunter: Empfaenger- statt Anrufer-Ton, beide
  Anrufrichtungen, Switchboard, fehlende Eintraege/Assets, Zusatzpuffer,
  Monster unveraendert, Geraeuschwiederholungen, Annahme und Abbruch.

Die mitgelieferte DLL unter artifacts/bin wurde mit diesen Tests geprueft.
artifacts/tests/verification.json ordnet den Teststand ueber den SHA256-Hash
eindeutig der jeweiligen DLL zu. Neue Builds erzeugen einen neuen Nachweis.

- Aufloesung echter Telefonkomponenten in der Unity-Szene bei fehlender Kontaktliste.
- Bereits zerstoerte AudioClips werden wie fehlende Assets behandelt.
- Build und Release aus einem neuen Verzeichnis mit Leerzeichen/Umlauten;
  eigener README-Inhalt landet im Paket, private Pfade bleiben draussen.
- Testinstaller am isolierten Profil: Installation, Update, Sicherung,
  Moduswechsel und Migration vom alten privaten Fix.

## Lobbytest bestanden

Der Nutzer hat am 23.10.2026 einen erfolgreichen Lobbytest der bereitgestellten
Fassung 1.0.0 bestaetigt. Der Test ist damit nicht mehr als offen vermerkt.
Diese Rueckmeldung stammt vom Nutzer; der Spielablauf wurde nicht automatisch
beobachtet. Welche einzelnen Konfigurationsvarianten dabei verwendet wurden,
wurde nicht angegeben.

Der automatische Nachweis in verification.json beschreibt ausschliesslich die
automatischen Tests der jeweiligen DLL. Der hier dokumentierte Lobbytest wird
nicht bei jedem neuen Build automatisch wiederholt oder erneut bestaetigt.
