# Terminal.Starter.CommandLine

Basis voor het maken van een terminal applicatie die gebruikt maakt
van [System.CommandLine](https://github.com/dotnet/command-line-api). Bevat functionaliteit om alle commands die zich
bevinden in een project te laden. Dit wordt gedaan door alle klassen die de klasse **Command** extenden binnen het
project te zoeken en te registreren.

## Installatie

- Voeg de NuGet package van dit project toe aan een .NET Console Applicatie.
- Verander de Main methode naar onderstaande code zonder hosting:

```c#
   public static async Task<int> Main(string[] args)
   {
       var services = new ServiceCollection();
       services.AddProjectCommands();
       services.AddDefaultCommands();
       return await services.RunTerminal(args);
   }
```

of wanneer gebruikt wordt gemaakt van **Microsoft.Extensions.Hosting** naar:

```c#
public static async Task Main(string[] args)
{
    using var host = CreateHostBuilder(args).Build();
    await host.RunAsync();
}

public static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddDefaultCommands();
            services.AddProjectCommands();
            services.RunTerminal(args);
        });
}
```

Met **AddDefaultCommands()** worden een aantal standaard commands toegevoegd die zich in dit project bevinden zoals
**list**.

Met **AddProjectCommands()** worden de commands toegevoegd die zich in het nieuwe project bevinden.

Met **AddAssemblyCommands(params Assembly[] assemblies)** kan gebruikt worden om commands uit een specifieke assembly te
laden.

- Creëer met onderstaand command een executable van de applicatie.

```powershell
dotnet publish --configuration Release --framework net5.0 --runtime win-x64 --self-contained true -p:PublishSingleFile = true
```

OF

- Creëer een dotnet tool NuGet package van de applicatie.

```xml

<PropertyGroup>
    <PackageId>{naam van applicatie}</PackageId>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
    <IsPackable>true</IsPackable>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>{naam van applicatie}</ToolCommandName>
</PropertyGroup>
```

Wanneer voor de tweede optie gekozen wordt, kan de applicatie (als deze is gepubliceerd) makkelijk worden geinstalleerd
met .NET CLI met onderstaand command.

```powershell
dotnet tool install -g { naam van applicatie }
```

Hierbij wordt de applicatie toegevoegd aan de PATH en is het mogelijk auto-complete te gebruiken.
