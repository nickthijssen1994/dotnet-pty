using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Terminal.Starter
{
	public sealed class TerminalShell
	{
		private readonly RootCommand _rootCommand = new();
		private Parser _parser;

		public TerminalShell(ServiceProvider serviceProvider, string? name = "", string? description = "")
		{
			if (!string.IsNullOrWhiteSpace(name)) _rootCommand.Name = name;

			_rootCommand.Description = description;
			RegisterRootCommand(serviceProvider);
			_parser = BuildParser();
		}

		public TerminalShell(ServiceProvider serviceProvider, RootCommand rootCommand)
		{
			_rootCommand = rootCommand;
			RegisterRootCommand(serviceProvider);
			_parser = BuildParser();
		}

		public async Task<int> Execute(string[] args)
		{
			return await _parser.InvokeAsync(args);
		}

		private void RegisterRootCommand(ServiceProvider serviceProvider)
		{
			List<Command> commands = new();
			foreach (var command in serviceProvider.GetServices<Command>())
			{
				commands.Add(command);
			}

			commands.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

			foreach (var command in commands)
			{
				_rootCommand.AddCommand(command);
			}
		}

		private Parser BuildParser()
		{
			var commandLineBuilder = new CommandLineBuilder(_rootCommand);

			_parser = commandLineBuilder.UseHost().UseDefaults().UseTypoCorrections(1).Build();

			return _parser;
		}
	}
}