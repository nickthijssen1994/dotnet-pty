using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Terminal.Starter.Printer;

namespace Terminal.Tool.Commands
{
	public class TimerNoCancelCommand : Command
	{
		private int _executionCount;

		public TimerNoCancelCommand() : base("timernc", "Timer with progressbar that cannot be cancelled.")
		{
			AddAlias("-tmnc");
			Handler = CommandHandler.Create(HandleCommand);
		}

		private async Task<int> HandleCommand()
		{
			try
			{
				ConsoleUtility.WriteProgressBar(0, 10);
				Console.WriteLine();
				while (_executionCount < 10)
				{
					_executionCount++;
					ConsoleUtility.WriteProgressBar(_executionCount, 10, true);
					Console.WriteLine("Timer Without Cancellation. Count: {0}", _executionCount);

					await Task.Delay(1000);
				}

				await Task.Delay(200);
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