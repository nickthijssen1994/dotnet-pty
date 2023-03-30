using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Terminal.Connector
{
	[Authorize]
	public class TerminalHub : Hub<ITerminalClient>
	{
		private readonly ILogger<TerminalHub> _logger;
		private readonly ITerminalController _terminalController;
		private static readonly Dictionary<string, string> _clientsTerminals = new();

		public TerminalHub(ITerminalController terminalController, ILogger<TerminalHub> logger)
		{
			_logger = logger;
			_terminalController = terminalController;
		}

		[Authorize(Policy = "TerminalPolicy")]
		[HubMethodName("SendSelectTerminal")]
		public async Task SendSelectTerminal(string input)
		{
			_logger.LogInformation("Select Terminal Message Received: [User]:[" + Context.User.Identity.Name +
			                       "] [Message]:[" +
			                       input + "] [Time]:[" + DateTime.Now + "]");

			if (_clientsTerminals.ContainsKey(Context.UserIdentifier))
			{
				_clientsTerminals[Context.UserIdentifier] = input;
			}
			else
			{
				_clientsTerminals.Add(Context.UserIdentifier, input);
			}
			await Groups.AddToGroupAsync(Context.UserIdentifier, input);
			// await _terminalController.SelectTerminal(input);
		}

		[Authorize(Policy = "TerminalPolicy")]
		[HubMethodName("SendRunCommand")]
		public async Task SendRunCommand(string input)
		{
			_logger.LogInformation("Run Command Message Received: [User]:[" + Context.User.Identity.Name +
			                       "] [Message]:[" +
			                       input + "] [Time]:[" + DateTime.Now + "]");
			await _terminalController.HandleStart(_clientsTerminals[Context.UserIdentifier], input);
		}

		[Authorize(Policy = "TerminalPolicy")]
		[HubMethodName("SendCommandLineInput")]
		public async Task SendCommandLineInput(string input)
		{
			_logger.LogInformation("CommandLine Input Message Received: [User]:[" + Context.User.Identity.Name +
			                       "] [Message]:[" + input + "] [Time]:[" + DateTime.Now + "]");
			await _terminalController.HandleInput(_clientsTerminals[Context.UserIdentifier], input);
		}

		[Authorize(Policy = "TerminalPolicy")]
		[HubMethodName("RequestCommandLineData")]
		public async Task RequestCommandLineData()
		{
			_logger.LogInformation("CommandLine Date Request Received: [User]:[" + Context.User.Identity.Name + "]");
			await Clients.Caller.ReceiveCommandLineData(_terminalController.GetCommandListData(_clientsTerminals[Context.UserIdentifier]));
		}

		[Authorize(Policy = "TerminalPolicy")]
		[HubMethodName("RequestTerminalExecutables")]
		public async Task RequestTerminalExecutables()
		{
			_logger.LogInformation("Terminal Executables Request Received: [User]:[" + Context.User.Identity.Name +
			                       "]");
			await Clients.Caller.ReceiveTerminalExecutables(_terminalController.GetTerminalExecutables());
		}

		[Authorize(Policy = "TerminalPolicy")]
		public override async Task OnConnectedAsync()
		{
			if (Context.User.Identity.IsAuthenticated)
			{
				_logger.LogInformation("Client Connected: [User]:[" + Context.User.Identity.Name + "]");

				await Groups.AddToGroupAsync(Context.UserIdentifier, "TerminalUsers");

				await Clients.Caller.ReceiveUsername(Context.User.Identity.Name);
				await Clients.Caller.ReceiveTerminalReset();
				if (_clientsTerminals.ContainsKey(Context.UserIdentifier))
				{
					await Clients.Caller.ReceiveTerminalOutput(_terminalController.GetOutputBuffer(_clientsTerminals[Context.UserIdentifier]));
				}
				await base.OnConnectedAsync();
			}
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			_logger.LogInformation("Client Disconnected");

			await Groups.RemoveFromGroupAsync(Context.UserIdentifier, "TerminalUsers");
			await base.OnDisconnectedAsync(exception);
		}
	}
}
