using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Terminal.Starter.Json.Models;

namespace Terminal.Starter.Models
{
	public class CommandModel : ICommandModel
	{
		public CommandModel(Command command)
		{
			name = command.Name;
			description = command.Description;
			aliases = new List<string>(command.Aliases);
			AddOptions(command.Options);
			AddArguments(command.Arguments);
			AddSubcommands(command.Children.OfType<Command>().Where(x => !x.IsHidden));
		}

		public string name { get; set; }
		public string? description { get; set; }
		public List<string> aliases { get; set; }
		public List<IOptionModel>? options { get; set; }
		public List<IArgumentModel>? arguments { get; set; }
		public List<ICommandModel>? commands { get; set; }

		private void AddSubcommands(IEnumerable<Command> subCommands)
		{
			commands = new List<ICommandModel>();
			foreach (var subcommand in subCommands)
			{
				commands.Add(new CommandModel(subcommand));
			}
		}

		private void AddOptions(IReadOnlyList<Option> _options)
		{
			options = new List<IOptionModel>();
			foreach (var option in _options)
			{
				options.Add(new OptionModel(option));
			}
		}

		private void AddArguments(IReadOnlyList<Argument> _arguments)
		{
			arguments = new List<IArgumentModel>();
			foreach (var argument in _arguments)
			{
				arguments.Add(new ArgumentModel(argument));
			}
		}
	}
}