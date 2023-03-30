using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Terminal.Starter.Json;
using Terminal.Starter.Models;
using Terminal.Starter.Printer;

namespace Terminal.Starter
{
	public class DefaultRootCommand : RootCommand
	{
		public DefaultRootCommand()
		{
			var jsonOption = new Option<bool>(
				"--json",
				description: "Generate json file of all commands",
				parseArgument: result =>
				{
					if (result.FindResultFor(this)?.Children.Count > 1)
					{
						result.ErrorMessage =
							"--json option cannot be combined with other commands, options or arguments.";
						return false;
					}

					return true;
				}
			);
			AddOption(jsonOption);

			var listCommand = new Command("list", "Print list of all available commands");
			listCommand.AddAlias("l");
			listCommand.AddAlias("-l");
			listCommand.AddAlias("ls");
			listCommand.AddAlias("-ls");

			listCommand.Handler = CommandHandler.Create((IConsole console) =>
			{
				try
				{
					var builder = new CommandWriter(console);

					List<Command> commands = Children.OfType<Command>().Where(x => !x.IsHidden).ToList();

					commands.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

					builder.Write(commands);
				}
				catch (Exception ex)
				{
					console.Out.WriteLine("Task Was Cancelled");
					return 1;
				}

				return 0;
			});
			AddCommand(listCommand);

			Handler = CommandHandler.Create(
				async (IConsole console, IHost host, bool json, CancellationToken token) =>
				{
					try
					{
						var builder = new CommandWriter(console);
						builder.Write(this);

						if (json)
						{
							var rootCommandModel = new RootCommandModel(this);
							JsonExtensions.Write(rootCommandModel);
						}

						Run(host);

						await Task.Delay(200, token);
					}
					catch (Exception ex)
					{
						console.Out.WriteLine("Task Was Cancelled");
						return 1;
					}

					return 0;
				});
		}

		private void Run(IHost host)
		{
			var serviceProvider = host.Services;
			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(typeof(CliCommandCollectionExtensions));
			logger.LogInformation("CommandLine Running");
		}
	}
}