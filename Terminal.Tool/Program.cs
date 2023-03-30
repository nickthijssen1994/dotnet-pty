using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Starter;

namespace Terminal.Tool
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddDefaultCommands();
			services.AddProjectCommands();
			var serviceProvider = services.BuildServiceProvider();

			var rootCommand = new DefaultRootCommand
			{
				Name = "terminal",
				Description = "Terminal Application Using System.CommandLine"
			};

			var terminal = serviceProvider.BuildTerminal(rootCommand);

			return await terminal.Execute(args);
		}
	}
}