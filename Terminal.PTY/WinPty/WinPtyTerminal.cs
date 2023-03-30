using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal.PTY.WinPty
{
	/// <summary>
	///     Uses winpty.dll which works on Windows XP through Windows 10 including server versions.
	///     Supports full VT escape sequence output like colors.
	///     More complicated implementation than ProcessPtyTerminal.
	/// </summary>
	public class WinPtyTerminal : ITerminal
	{
		private const string CtrlCCommand = "\x3";
		private readonly ITerminalOptions _options;
		private StreamWriter _consoleInputWriter;
		private IPtyConnection _terminal;

		/// <summary>
		/// </summary>
		/// <param name="options"></param>
		/// <param name="logger"></param>
		public WinPtyTerminal(ITerminalOptions options)
		{
			_options = options;
		}

		/// <inheritdoc />
		public ITerminalOptions Options
		{
			get { return _options; }
		}

		/// <inheritdoc />
		public bool Running { get; private set; }

		/// <inheritdoc />
		public event EventHandler<string> OutputReady;

		/// <inheritdoc />
		public event EventHandler<string> ErrorOutputReady;

		/// <inheritdoc />
		public event EventHandler ProcessExited;

		/// <inheritdoc />
		public void ExecuteAsync(string executable, string[] arguments, CancellationToken cancellationToken)
		{
			Task.Run(async () => await ConnectToTerminal(executable, arguments, cancellationToken), cancellationToken);
		}

		/// <inheritdoc />
		public void Write(string input)
		{
			if (input == null) return;
			if (_consoleInputWriter == null)
				throw new InvalidOperationException(
					"There is no writer attached to a pseudoconsole. Have you called Start on this instance yet?");
			_consoleInputWriter.Write(input);
		}

		private async Task ConnectToTerminal(string executable, string[] arguments, CancellationToken cancellationToken)
		{
			var encoding = new UTF8Encoding();
			var app = executable;
			var options = new PtyOptions
			{
				Cols = 150,
				Rows = 50,
				Cwd = Environment.CurrentDirectory,
				App = app,
				CommandLine = arguments,
				VerbatimCommandLine = true,
				ForceWinPty = true,
				Environment = _options.EnvironmentVariables
			};

			_terminal = await PtyProvider.SpawnAsync(options, cancellationToken);
			Running = true;

			var processExitedTcs = new TaskCompletionSource<uint>();
			_terminal.ProcessExited += (sender, e) => processExitedTcs.TrySetResult((uint)_terminal.ExitCode);
			_terminal.ProcessExited += Process_Exited;

			string GetTerminalExitCode()
			{
				return processExitedTcs.Task.IsCompleted
					? $". Terminal process has exited with exit code {processExitedTcs.Task.GetAwaiter().GetResult()}."
					: string.Empty;
			}

			var outputTask = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			var output = string.Empty;
			var checkTerminalOutputAsync = Task.Run(async () =>
			{
				using var reader = new StreamReader(_terminal.ReaderStream);
				var standardOutput = new StringBuilder();

				var buffer = new byte[4096];

				while (!cancellationToken.IsCancellationRequested && !processExitedTcs.Task.IsCompleted)
				{
					standardOutput.Clear();
					var count = await _terminal.ReaderStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
					if (count == 0) break;

					outputTask.TrySetResult(null);

					output = encoding.GetString(buffer, 0, count);
					standardOutput.Append(output);
					OnStandardTextReceived(output);
				}

				outputTask.TrySetCanceled();
				return false;
			});

			_consoleInputWriter = new StreamWriter(_terminal.WriterStream)
			{
				AutoFlush = true
			};
			ForwardCtrlC(_consoleInputWriter);

			try
			{
				await outputTask.Task;
			}
			catch (OperationCanceledException exception)
			{
				throw new InvalidOperationException(
					$"Could not get any output from terminal{GetTerminalExitCode()}",
					exception);
			}

			try
			{
				await checkTerminalOutputAsync;
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException(
					$"Could not get expected data from terminal.{GetTerminalExitCode()} Actual terminal output:\n{output}",
					exception);
			}

			using (cancellationToken.Register(() => processExitedTcs.TrySetCanceled(cancellationToken)))
			{
				var exitCode = await processExitedTcs.Task;
			}

			_terminal.WaitForExit(Timeout.Infinite);
			_terminal.Dispose();
		}

		private async void ReadOutputAsync()
		{
			var encoding = new UTF8Encoding();
			using var reader = new StreamReader(_terminal.ReaderStream);
			var standardOutput = new StringBuilder();
			var buffer = new byte[4096];

			while (Running)
			{
				standardOutput.Clear();
				var count = await _terminal.ReaderStream.ReadAsync(buffer, 0, buffer.Length);
				if (count > 0)
				{
					standardOutput.Append(encoding.GetString(buffer, 0, count));
					OnStandardTextReceived(standardOutput.ToString());
				}
			}
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			Running = false;
			ProcessExited?.Invoke(this, EventArgs.Empty);
		}

		private void OnStandardTextReceived(string e)
		{
			var handler = OutputReady;
			handler?.Invoke(this, e);
		}

		/// <summary>
		///     Don't let ctrl-c kill the terminal, it should be sent to the process in the terminal.
		/// </summary>
		private void ForwardCtrlC(StreamWriter writer)
		{
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				writer.Write(CtrlCCommand);
			};
		}
	}
}
