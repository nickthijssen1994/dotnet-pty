using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Terminal.Starter.Commands
{
	public sealed class ConsoleSettingsCommand : Command
	{
		public ConsoleSettingsCommand() : base("settings", "Print settings of console and OS.")
		{
			AddAlias("-st");

			Handler = CommandHandler.Create((CancellationToken token) => HandleCommand(token));
		}

		private async Task<int> HandleCommand(CancellationToken token)
		{
			Console.WriteLine("System.Console Settings:");
			Console.WriteLine($"Input Encoding: {Console.InputEncoding.EncodingName}");
			Console.WriteLine($"Output Encoding: {Console.OutputEncoding.EncodingName}");

			Console.WriteLine();
			var settings = AnsiConsole.Profile;
			Console.WriteLine("Spectre.Console Settings:");
			AnsiConsole.MarkupLine($"[yellow]Encoding[/]: {settings.Encoding.EncodingName}");
			AnsiConsole.MarkupLine($"[yellow]Ansi[/]: {settings.Capabilities.Ansi}");
			AnsiConsole.MarkupLine($"[yellow]Interactive[/]: {settings.Capabilities.Interactive}");
			AnsiConsole.MarkupLine($"[yellow]Links[/]: {settings.Capabilities.Links}");
			AnsiConsole.MarkupLine($"[yellow]Legacy[/]: {settings.Capabilities.Legacy}");
			AnsiConsole.MarkupLine($"[yellow]ColorSystem[/]: {settings.Capabilities.ColorSystem.ToString()}");
			AnsiConsole.MarkupLine($"[yellow]Unicode[/]: {settings.Capabilities.Unicode}");
			AnsiConsole.MarkupLine($"[yellow]Width[/]: {settings.Width}");
			AnsiConsole.MarkupLine($"[yellow]Height[/]: {settings.Height}");

			await Task.Delay(200, token);

			return 0;
		}
	}
}