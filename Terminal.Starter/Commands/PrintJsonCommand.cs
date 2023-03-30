using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal.Starter.Commands
{
	public sealed class PrintJsonCommand : Command
	{
		public PrintJsonCommand() : base("print-json", "Print content of commands.json file")
		{
			AddAlias("-pj");

			Handler = CommandHandler.Create((CancellationToken token) => HandleCommand(token));
		}

		private async Task<int> HandleCommand(CancellationToken token)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands.json");

			Console.WriteLine(File.Exists(path) ? File.ReadAllText(path) : "");

			await Task.Delay(200, token);

			return 0;
		}
	}
}