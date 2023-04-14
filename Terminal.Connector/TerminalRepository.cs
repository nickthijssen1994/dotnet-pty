using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Terminal.PTY;
using Terminal.PTY.WinPty;

namespace Terminal.Connector
{
	public class TerminalRepository : ITerminalRepository
	{
		private readonly ILogger<TerminalRepository> _logger;
		private readonly ITerminalOptions _options;
		private readonly ITerminalController _terminalController;
		private readonly CancellationToken _token;
		private readonly CancellationTokenSource _tokenSource = new();
		private readonly Dictionary<string, ITerminal> _terminals = new();
		private readonly ITerminal _terminal;

		public TerminalRepository(ITerminalController terminalController,
			ITerminalOptions options, ILogger<TerminalRepository> logger)
		{
			_terminalController = terminalController;
			_options = options;
			_logger = logger;
			_token = _tokenSource.Token;
			RegisterTerminalExecutables();
		}

		public void SelectTerminal(string executable)
		{
			_options.Executable = _terminals[executable].Options.Executable;
		}

		public void HandleStart(string executable, string arguments)
		{
			if (string.IsNullOrWhiteSpace(arguments)) arguments = _options.Arguments;

			if (!_terminals[executable].Running)
			{
				ClearBuffer(executable);
				Task.Run(async () => { await _terminalController.HandleTerminalReset(executable); }, _token);

				Task.Run(
					() => _terminals[executable].ExecuteAsync(
						GetExecutablePath(executable), arguments.Split(' '), _token),
					_token);
			}
			else
			{
				_logger.LogInformation("Cannot start terminal because here is already one running.");
			}
		}

		public void WriteToPseudoConsole(string executable, string input)
		{
			if (_terminals[executable].Running) _terminals[executable].Write(input);
		}

		public string GetOutputBuffer(string executable)
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			var path = Path.Combine(baseDirectory, "last-buffer.txt");
			return File.Exists(path) ? File.ReadAllText(path) : "";
		}

		public string GetCommandListData(string executable)
		{
			var path = Path.Combine(_options.Directory, "commands.json");
			return File.Exists(path) ? File.ReadAllText(path) : "";
		}

		private void RegisterTerminalExecutables()
		{
			// var terminal = new WinPtyTerminal(_options);
			// terminal.OutputReady += Terminal_OutputReady;
			// terminal.ProcessExited += Terminal_Finished;
			// _terminals.Add("PowerShell", terminal);
			_options.Executable = "Terminal.Tool.exe";
			var powershell =
				new WinPtyTerminal(_options);
			powershell.OutputReady += Terminal_OutputReady;
			powershell.ProcessExited += Terminal_Finished;
			_terminals.Add("Terminal.Tool.exe", powershell);
		}

		public string GetTerminalExecutables()
		{
			var executables = new List<string>();
			foreach (var terminal in _terminals)
			{
				executables.Add(terminal.Key);
			}
			var json = JsonConvert.SerializeObject(executables, Formatting.Indented);
			return json;
		}

		private void Terminal_OutputReady(object sender, string e)
		{
			Task.Run(async () => await CopyConsoleToWindow( ((ITerminal)sender).Options.Executable,e), _token);
		}

		private void Terminal_Finished(object sender, EventArgs e)
		{
			Task.Run(async () => await CopyConsoleToWindow(((ITerminal)sender).Options.Executable,"PROCESS FINISHED"), _token);
		}

		private async Task CopyConsoleToWindow(string executable, string output)
		{
			WriteToBuffer(executable, output);
			await _terminalController.HandleOutput(executable, output);
		}

		private void WriteToBuffer(string executable, string output)
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			using StreamWriter file = new(Path.Combine(baseDirectory, "last-buffer.txt"), true);
			file.Write(output);
		}

		private void ClearBuffer(string executable)
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			using StreamWriter file = new(Path.Combine(baseDirectory, "last-buffer.txt"), false);
			file.Write("");
		}

		private string GetExecutablePath(string executable)
		{
			var result = Environment
				.GetEnvironmentVariable("PATH")
				?.Split(';')
				.FirstOrDefault(s => File.Exists(Path.Combine(s, executable)));

			if (File.Exists(Path.Combine(_options.Directory, executable)))
			{
				_logger.LogInformation("Found Executable in Directory: " +
				                       Path.Combine(_options.Directory, executable));
				return Path.Combine(_options.Directory, executable);
			}

			if (result != null)
			{
				_logger.LogInformation("Found Executable In PATH: " + Path.Combine(result, executable));
				return Path.Combine(result, executable);
			}

			_logger.LogInformation("Could Not Find Executable In Directory Or Path");
			_terminalController.HandleOutput(executable,"Could Not Find Executable");
			return "";
		}
	}
}
