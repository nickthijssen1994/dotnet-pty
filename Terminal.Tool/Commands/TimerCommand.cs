using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Terminal.Starter.Printer;

namespace Terminal.Tool.Commands
{
	public class TimerCommand : Command
	{
		private int _executionCount;

		public TimerCommand() : base("timer", "Timer with progressbar that can be cancelled with Ctrl C.")
		{
			AddAlias("-tm");
			Handler = CommandHandler.Create((CancellationToken token) => HandleCommand(token));
		}

		private async Task<int> HandleCommand(CancellationToken token)
		{
			try
			{
				ConsoleUtility.WriteProgressBar(0, 10);
				Console.WriteLine();
				while (_executionCount < 10)
				{
					_executionCount++;

					ConsoleUtility.WriteProgressBar(_executionCount, 10, true);
					Console.WriteLine("Timer With Cancellation. Count: {0}", _executionCount);

					await Task.Delay(1000, token);
				}

				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				AnsiConsole.WriteException(ex,
					ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
					ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}
	}
}