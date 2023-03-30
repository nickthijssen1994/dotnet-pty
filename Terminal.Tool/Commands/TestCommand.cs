using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Terminal.Tool.Commands
{
	public class TestCommand : Command
	{
		public TestCommand() : base("test", "Test command with subcommands.")
		{
			var argument = new Argument("testargument");
			AddArgument(argument);
			var option = new Option<bool>("--verbose");
			option.AddAlias("-v");
			AddOption(option);
			var subcommand = new Command("subtest");
			var subsubcommand = new Command("subsubtest");
			subcommand.AddCommand(subsubcommand);
			AddCommand(subcommand);
			AddAlias("-t");
			Handler = CommandHandler.Create(HandleCommand);
		}

		private async Task<int> HandleCommand()
		{
			try
			{
				Console.WriteLine("Test Message");
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