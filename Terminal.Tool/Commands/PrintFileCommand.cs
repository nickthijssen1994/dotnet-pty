using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Terminal.Tool.Commands
{
	public class PrintFileCommand : Command
	{
		public PrintFileCommand() : base("print-file", "Print content of a file. File can contain ANSI escape codes.")
		{
			var file = new Option<string>("--file")
			{
				Name = "file",
				Description = "Path of file to print",
				IsRequired = false
			};

			AddOption(file);
			AddAlias("-pf");
			Handler = CommandHandler.Create((string file, CancellationToken token) =>
				HandleCommand(file, token));
		}

		private async Task<int> HandleCommand(string file, CancellationToken token)
		{
			try
			{
				var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				var path = Path.Combine(baseDirectory, file);
				var content = File.Exists(path) ? await File.ReadAllTextAsync(path, token) : "";
				AnsiConsole.Write(content);
				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}
	}
}