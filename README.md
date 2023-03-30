# Terminal

[![pipeline status](https://git.nl/nick.thijssen/terminal/badges/master/pipeline.svg)](https://git.nl/nick.thijssen/terminal/-/commits/master)

## Projecten

Dit project bestaat globaal uit drie onderdelen die samen het nieuwe Terminal project vormen, namelijk:

- Terminal.Starter.CommandLine
- Terminal.Backend(.Kestrel)
- Terminal.Frontend

Alle andere projecten zijn sub onderdelen voor andere projecten, voorbeelden van implementaties of dienden voor
onderzoek.

**Terminal.Starter** en **Terminal.Abstractions** bevatten het orginele project.
**Terminal.Tool** is hier een voorbeeld implementatie van.

## Terminal.Starter.CommandLine

Dit is een library die alle basis functionaliteit bevat om een uitgebreide terminal applicatie te maken. Maakt gebruik
van het nieuwe [System.CommandLine](https://github.com/dotnet/command-line-api) project.

Voorbeelden van implementaties zijn **Terminal.Tool.CommandLine** en **Terminal.Tool.Hosting**.

## Terminal.Backend en Terminal.Backend.Kestrel

Backend applicaties die het mogelijk maaken om met een console applicatie te verbinden en input en output te laten
verlopen door middel van SignalR (qua principe gelijk aan een ssh server).

De backend staat grotendeels los van terminal applicaties die gemaakt zijn met
**Terminal.Starter.CommandLine**. Er kan bijvoorbeeld ook verbonden worden met PowerShell en CMD.exe.

De projecten **Terminal.PTY**, **Terminal.Connector** en **Terminal.Connector.SignalR** worden gebruikt door deze
projecten.

De backend is gemaakt om op een Windows Server te draaien en gebruikt Active Directory om gebruikers te authenticeren en
authorizeren.

**Terminal.Backend**
gebruikt [HTTP.sys](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/httpsys?view=aspnetcore-5.0) als
webserver en is als Windows Service te draaien. Deze versie kan niet via IIS draaien. **ConPty** en **WinPty** werken
allebei op deze versie.

**Terminal.Backend.Kestrel** is een alternatieve versie van **Terminal.Backend** die gedeployed kan worden via IIS. Deze
versie is makkelijker te deployen en configureren dan **Terminal.Backend**, maar werkt alleen met **WinPty** en niet
met **ConPty** om met terminal applicaties te verbinden. Wanneer **ConPty** gebruikt wordt, wordt output van terminal
applicaties gepiped naar IIS in plaats van naar de backend applicatie. Input van de backend naar de terminal wordt wel
goed doorgegeven. De oorzaak hiervan is niet helemaal duidelijk, maar heeft waarschijnlijk te maken met het volgende:

**ConPty** gebruikt de `kernel32.dll` uit `C:\Windows\System32`. Met deze dll kunnen processen worden gestart, gestopt
en input en output worden doorgegeven door een ander process. Met welk process er moet worden gecommuniceerd, wordt
gekozen op basis van zijn PID (Process ID). Wanneer de backend vraagt om de terminal op te starten, krijgt deze de PID
van de terminal terug zodat hij weet naar welk process input gestuurd moet worden. Andersom wordt output van de terminal
gelinkt aan de PID van de backend applicatie. Er onstaat alleen een probleem wanneer de backend in IIS draait. Wanneer
de backend en terminal als zelfstandige processen draaien, is de flow bij input...

`Backend -> kernel32.dll -> Terminal`

en bij output...

`Terminal -> kernel32.dll -> Backend`

Wanneer de backend in IIS draait is dit echter...

`Backend -> IIS -> kernel32.dll -> Terminal`

...bij input en...

`Terminal -> kernel32.dll -> IIS -> Backend`

...bij output.

Output van de terminal wordt doorgegeven aan het IIS process en niet aan het backend process.

Om een voorbeeld te zien van dit probleem, moet je in de `appsettings.json` van het **Terminal.Backend.Kestrel**
project als `PseudoConsoleType` `ConPtyTerminal` opgeven en deze opstarten via IIS Express (in Rider of Visual Studio).
Wanneer je vervolgens via de frontend een command uitvoert, zie je de output niet in de frontend, maar in de terminal
van IIS Express.

## Terminal.Frontend

JavaScript library die kan worden toegevoegd aan frontends om met **Terminal.Backend** te kunnen communiceren en om een
terminal venster weer te kunnen geven. Gebruikt [@microsoft/signalr](https://www.npmjs.com/package/@microsoft/signalr)
voor de communicatie en [XTerm.JS](https://github.com/xtermjs/xterm.js) voor het weergeven van een terminalvenster.

**Terminal.Client** is een voorbeeld voor een simpele NodeJS frontend die deze library gebruikt en
**Terminal.Client.Angular** dient als voorbeeld voor het gebruik met Angular.

## Gebruik

Voor de terminal en de backend is een Windows Server versie 2012 of hoger nodig. Deze applicaties moeten zich op
dezelfde machine bevinden.

De frontend moet gedeployed worden in IIS. Windows Authentication moet in IIS aanstaan. De frontend hoeft niet op
dezelfde server te staan als de backend, al heeft dit wel de voorkeur.

De backend gebruikt Windows Authentication voor het afschermen van de SignalR endpoint. Specifiek houdt dit in dat de
authenticatie mechanismen Negotiate, Kerberos en NTLM worden gebruikt. Ondersteuning voor deze mechanismen verschilt
tussen browsers en zij worden niet altijd hetzelfde geimplementeerd waardoor gebruik kan verschillen.

## Onderzoek en Inspiratie Projecten

* [Microsoft Terminal](https://github.com/microsoft/terminal)
* [Node-PTY](https://github.com/microsoft/node-pty)
* [System.CommandLine](https://github.com/dotnet/command-line-api)
* [XTerm.JS](https://github.com/xtermjs/xterm.js)
