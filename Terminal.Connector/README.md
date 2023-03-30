# Terminal.Connector

Bevat de code voor het beheren van één of meerdere terminals. Maakt het mogelijk een terminal op de achtergrond op te
starten, te luisteren naar de output hiervan en input door te geven aan de terminal. Maakt gebruik van
**Terminal.PTY** voor het juist doorgeven van input en output.

De interface **ITerminalController** kan gebruikt worden om specifieke listeners te maken. Zie het project **
Terminal.Connector.SignalR** voor een implementatie voorbeeld met SignalR. SignalR of andere real-time communicatie
methodes worden aangeraden, maar het is ook mogelijk om dit bijv. via REST te doen.

## Gebruik

Om **Terminal.Connector** te gebruiken moet een implementatie van **ITerminalController** worden toegevoegd en moet
met **AddTerminalRunner** worden aangegeven welke console applicatie er moet worden opgestart. Dit kan bijv. worden
gedaan door het in **appsettings.json** te zetten.

```c#
services.AddSingleton<ITerminalController, SignalRTerminalController>();
services.AddTerminalRunner(new TerminalOptions
	{
		PseudoConsoleType = Configuration["Terminal:PseudoConsoleType"],
		Directory = Configuration["Terminal:Directory"],
		Executable = Configuration["Terminal:Executable"],
		Arguments = Configuration["Terminal:Arguments"]
	});
```

## Werking

De klassen **TerminalRunner** en **TerminalRepository** zorgen voor het correct opstarten en afsluiten van een console
applicatie. Door de methode **WriteToPseudoConsole(string input)** van de **TerminalRepository** aan te spreken kan
input worden doorgegeven aan de applicatie. Bij nieuwe output geeft **TerminalRepository** deze door aan de
geregistreerde controller.

# Terminal.Connector.SignalR

Dit project bevat een implementatie van **Terminal.Connector** die gebruikt maakt
van [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-5.0). Hier is de SignalR
Hub te vinden die verschillende methodes bevat om output van terminals door te geven aan clients en input door te sturen
naar de terminal.

Methodes of de gehele Hub kunnen hier worden afgeschermd door middel van de **[Authorize]** annotatie zoals dat ook kan
bij bijv. REST Controllers.

## Methodes

### Van Client naar Server

```c#
	[HubMethodName("ReceiveTerminalOutput")]
		Task ReceiveTerminalOutput(string output);

		[HubMethodName("ReceiveCommandLineData")]
		Task ReceiveCommandLineData(string output);

		[HubMethodName("ReceiveUsername")]
		Task ReceiveUsername(string username);

		[HubMethodName("ReceiveTerminalReset")]
		Task ReceiveTerminalReset();


    [Authorize(Policy = "TerminalPolicy")]
    public override async Task OnConnectedAsync(){}
```

Wordt aangesproken bij een nieuwe verbinding. Checked gebruikersrechten door middel van een Policy. Als dit faalt, wordt
de verbinding niet aangegaan. Als een gebruiker nog niet is ingelogd, wordt een login promt getoont in de browser.

```c#
    [HubMethodName("SendRunCommand")]
    public async Task SendRunCommand(string input)
```

Via deze methode kunnen de argumenten waarmee de terminal moet starten worden gestuurd naar de backend. De terminal
wordt vervolgens op de achtergrond opgestart met als argumenten de gegeven input.

```c#
    [HubMethodName("SendCommandLineInput")]
    public async Task SendCommandLineInput(string input)
```

Hiermee kan input doorgegeven worden aan een terminal applicatie terwijl deze draait. Deze methode kan bijvoorbeeld
gebruikt worden bij een `Console.ReadLine()`. In de huidige implementatie wordt input rechtstreeks doorgegeven aan de
console applicatie. Dit houdt in het geval van bijv. een `ReadLine()` in dat de input gewijzigd kan worden zolang er
niet een `Enter` wordt ingevoerd. Clients kunnen input in zijn geheel sturen door de methode één keer aan te spreken, of
in delen door deze methode meerde keren aan te spreken.

```c#
    [HubMethodName("RequestCommandLineData")]
    public async Task RequestCommandLineData(){}
```

Met het aanspreken van deze methode, kan het JSON bestand opgevraagd worden waarin het gebruik van de console applicatie
beschreven staat. Deze methode werkt alleen goed in combinatie met een console applicatie die gebruik maakt van de
**Terminal.Starter.CommandLine** library. Deze library bevat namelijk de functionaliteit om een JSON file te genereren
waarin alle commands en hun gebruik beschreven staat. De backend zal proberen dit bestand op te halen en door middel van
de methode `ReceiveCommandLineData()` dit terugsturen naar de client.

### Van Server naar Client

```c#
    [HubMethodName("ReceiveTerminalOutput")]
    Task ReceiveTerminalOutput(string output);
```

Methode die wordt aangesproken door backend wanneer er nieuwe output van de terminal is. Output kan hier bestaat uit één
stuk tekst of uit enkele karakters. Bij elke wijziging van output van de terminal, wordt deze methode aangesproken. Dit
maakt het mogelijk terminal output in real time te laten zien. De uiteindelijke output bevat ANSI escape codes die onder
andere informatie bevatten over de layout en kleur. Met bijvoorbeeld [XTerm.JS](https://xtermjs.org/)
kan deze output worden weergegeven.

```c#
    [HubMethodName("ReceiveCommandLineData")]
    Task ReceiveCommandLineData(string output);
```

Deze methode is alleen bruikbaar bij console applicaties die gebruik maken van **Terminal.Starter.CommandLine**. Hierin
zit namelijk de mogelijkheid om alle informatie over de beschikbare commands op te slaan als JSON bestand. Deze methode
haalt het bestand **commands.json** op uit de map waar de console applicatie in staat en stuurt de inhoud hiervan terug.

```c#
    [HubMethodName("ReceiveUsername")]
    Task ReceiveUsername(string username);
```

Methode om de gebruikersnaam van een ingelogde gebruiker op te halen. Doordat er gebruik gemaakt wordt van Windows
Authenticatie is dit in het formaat **DOMEIN\user**.

```c#
    [HubMethodName("ReceiveTerminalReset")]
    Task ReceiveTerminalReset();
```

Deze methode maakt het mogelijk om aan clients door te geven dat een nieuwe terminal of command is uitgevoerd. Deze
methode wordt aangesproken vóórdat een nieuwe terminal wordt opgestart en kan bijvoorbeeld door een client gebruikt
worden om het output scherm te legen. 

