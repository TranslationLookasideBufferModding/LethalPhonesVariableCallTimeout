# Verhalten und Konfiguration

Die neue eigenstaendige Mod verwendet die Plugin-ID lethalphones.calltimeout.
Ihre Datei ist BepInEx/config/lethalphones.calltimeout.cfg.
Der alte private.phones.calltimeout-Zusatzfix muss entfernt oder deaktiviert sein.

```ini
[General]
Enabled = true

[Calls]
Mode = Fixed
FixedSeconds = 45
MaximumRingtoneSeconds = 300
RingtoneGraceSeconds = 0
```

## Fixed

Unbeantwortete Anrufe von Spielern und Switchboard warten FixedSeconds lang.
Zulaessig sind 0,1 bis 3600 Sekunden. Standard ist 45.

## RingtoneLength

Mode = RingtoneLength richtet den Timer nach dem Klingelton des ANGERUFENEN
Telefons. Die bereits von LethalPhones verteilte Telefonliste liefert die
Zielnummer und die Klingelton-ID. Der Anrufer liest die Laenge aus seiner
lokalen Kopie desselben AudioClips.
Falls die Telefonliste noch unvollstaendig ist, werden die bereits vorhandenen
Telefonkomponenten in der laufenden Szene nach der Zielnummer durchsucht.

- Beim Switchboard wird dessen eigener Ring-Clip verwendet.
- Fehlt der angegebene Custom-Ton, wird wie bei LethalPhones der Standardton genutzt.
- Fehlt auch ein brauchbarer Standardton oder ist das Ziel noch nicht in der
  Telefonliste, wird FixedSeconds als Ersatzwartezeit verwendet.
- MaximumRingtoneSeconds begrenzt die Klingelton-basierte Wartezeit. Standard 300.
- RingtoneGraceSeconds addiert optional 0 bis 10 Sekunden, etwa um Netzwerklatenz
  auszugleichen. Standard 0. Der Timer beginnt beim Anrufer; der Ton beginnt beim
  Empfaenger nach Uebertragung. Es ist deshalb kein samplegenauer Audio-Ende-Trigger.
- Eine Mindestwartezeit von 0,1 Sekunden verhindert einen ungueltigen Null-Timer.

Beispiel: Ein 17,25-Sekunden-Ton ergibt ohne Zusatzpuffer einen 17,25-Sekunden-Timer.
Ein 45-Sekunden-Ton ergibt einen 45-Sekunden-Timer. Andere Dateien bleiben jeweils
bei ihrer eigenen Laenge. Die Mod schneidet und konvertiert keine Audiodateien.

## Mehrspieler und Grenzen

Jeder Anrufer benoetigt die Mod. Nutzt auf allen Clients denselben Modus,
dieselben Einstellungen und dieselben Klingeltondateien. Es wird kein eigener
Netzwerkdienst eingefuehrt. Unterschiedliche lokale Dateien unter derselben ID
koennen deshalb unterschiedliche Laengen ergeben.

Annehmen, Ablehnen und manuelles Auflegen duerfen jederzeit frueher beenden.
Anrufe, die Monster selbst ausloesen, erhalten keinen neuen Timer. Die
Geraeuschmeldungen an die Monster-KI werden fuer laenger klingelnde Spieler-
und Switchboard-Telefone weitergefuehrt.

Die Mod prueft die erwarteten Methoden und den bisherigen 15-Sekunden-Timer.
Passt das Layout nicht, werden ihre Patches zurueckgenommen und der Grund geloggt.
Die angepasste Version wurde gegen LethalPhones 1.3.23 vorbereitet. Spaetere
Modversionen benoetigen gegebenenfalls eine erneute Kompatibilitaetspruefung.
