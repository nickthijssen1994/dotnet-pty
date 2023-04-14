using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Terminal.Connector
{
	public class SignalRTerminalController : ITerminalController
	{
		private readonly ILogger<SignalRTerminalController> _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IHubContext<TerminalHub, ITerminalClient> _terminalHub;

		public SignalRTerminalController(IServiceScopeFactory scopeFactory,
			IHubContext<TerminalHub, ITerminalClient> terminalHub, ILogger<SignalRTerminalController> logger)
		{
			_logger = logger;
			_terminalHub = terminalHub;
			_scopeFactory = scopeFactory;
		}

		public async Task HandleStart(string executable,string arguments)
		{
			_logger.LogInformation("Starting Terminal");

			await Task.Run(() =>
			{
				using var scope = _scopeFactory.CreateScope();
				var terminalRepository = scope.ServiceProvider.GetRequiredService<ITerminalRepository>();
				terminalRepository.HandleStart(executable, arguments);
			});
		}

		public async Task HandleTerminalReset(string executable)
		{
			await _terminalHub.Clients.Group(executable).ReceiveTerminalReset();
		}

		/// <inheritdoc />
		public async Task SelectTerminal(string input)
		{
			using var scope = _scopeFactory.CreateScope();
			var terminalRepository = scope.ServiceProvider.GetRequiredService<ITerminalRepository>();
			await Task.Run(() => terminalRepository.SelectTerminal(input));
		}

		public async Task HandleInput(string executable,string input)
		{
			using var scope = _scopeFactory.CreateScope();
			var terminalRepository = scope.ServiceProvider.GetRequiredService<ITerminalRepository>();
			await Task.Run(() => terminalRepository.WriteToPseudoConsole(executable, input));
		}

		public async Task HandleOutput(string executable,string output)
		{
			await _terminalHub.Clients.Group("TerminalUsers").ReceiveTerminalOutput(output);
		}

		public string GetOutputBuffer(string executable)
		{
			using var scope = _scopeFactory.CreateScope();
			var terminalRepository = scope.ServiceProvider.GetRequiredService<ITerminalRepository>();
			return terminalRepository.GetOutputBuffer(executable);
		}

		public string GetCommandListData(string executable)
		{
			using var scope = _scopeFactory.CreateScope();
			var terminalRepository = scope.ServiceProvider.GetRequiredService<ITerminalRepository>();
			return terminalRepository.GetCommandListData(executable);
		}

		public string GetTerminalExecutables()
		{
			using var scope = _scopeFactory.CreateScope();
			var terminalRepository = scope.ServiceProvider.GetRequiredService<ITerminalRepository>();
			return terminalRepository.GetTerminalExecutables();
		}
	}
}
