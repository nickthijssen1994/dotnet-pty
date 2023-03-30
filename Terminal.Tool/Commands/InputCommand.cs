using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal.Tool.Commands
{
	public class InputCommand : Command
	{
		public InputCommand() : base("input", "Example for ReadLine and ReadKey input.")
		{
			AddAlias("-i");

			Handler = CommandHandler.Create((CancellationToken token) => HandleCommand(token));
		}

		private async Task<int> HandleCommand(CancellationToken token)
		{
			try
			{
				Console.Write("Please enter your name: ");
				var name = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(name)) name = "Anonymous";
				Console.WriteLine($"Hallo {name}!");
				Console.WriteLine("Do you want to proceed? (Y/n)");

				var inputGiven = false;
				while (!inputGiven)
				{
					var answer = Console.ReadKey(false).Key;
					if (answer != ConsoleKey.Enter) Console.WriteLine();
					if (answer == ConsoleKey.Y)
					{
						Console.WriteLine("Proceeding with execution");
						inputGiven = true;
					}
					else if (answer == ConsoleKey.N)
					{
						Console.WriteLine("Cancelling execution");
						inputGiven = true;
					}
					else
					{
						Console.WriteLine("Please answer yes or no");
					}
				}

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