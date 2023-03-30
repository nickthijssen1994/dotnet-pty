using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Terminal.PTY.ConPty.Processes;
using static Terminal.PTY.ConPty.Native.ConsoleApi;

namespace Terminal.PTY.ConPty
{
	/// <summary>
	///     Uses the latest version of conpty which is only available on Windows 10 1903 (build 18362) or later
	///     Supports full VT escape sequence output like colors.
	///     More complicated implementation than ProcessPtyTerminal.
	/// </summary>
	public class ConPtyTerminal : ITerminal
	{
		private const string CtrlCCommand = "\x3";
		private readonly ITerminalOptions _options;
		private SafeFileHandle _consoleInputPipeWriteHandle;
		private StreamWriter _consoleInputWriter;
		private PseudoConsolePipe _inputPipe;
		private PseudoConsolePipe _outputPipe;
		private Process _process;
		private PseudoConsole _pseudoConsole;

		/// <summary>
		/// </summary>
		/// <param name="logger"></param>
		public ConPtyTerminal(ITerminalOptions options)
		{
			_options = options;
		}

		/// <inheritdoc />
		public ITerminalOptions Options
		{
			get { return _options; }
		}

		/// <summary>
		///     A stream of VT-100-enabled output from the console.
		/// </summary>
		private FileStream ConsoleOutStream { get; set; }

		/// <inheritdoc />
		public event EventHandler<string> OutputReady;

		/// <inheritdoc />
		public event EventHandler<string> ErrorOutputReady;

		/// <inheritdoc />
		public event EventHandler ProcessExited;

		/// <inheritdoc />
		public bool Running { get; private set; }

		/// <summary>
		///     Start the psuedoconsole and run the process as shown in
		///     https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#creating-the-pseudoconsole
		/// </summary>
		/// <param name="executable">the command to run, e.g. cmd.exe</param>
		/// <param name="arguments"></param>
		/// <param name="cancellationToken"></param>
		public void ExecuteAsync(string executable, string[] arguments, CancellationToken cancellationToken)
		{
			_inputPipe = new PseudoConsolePipe();
			_outputPipe = new PseudoConsolePipe();
			_pseudoConsole = PseudoConsole.Create(_inputPipe.ReadSide, _outputPipe.WriteSide, 150, 100);

			_process = ProcessFactory.Start(executable + " " + string.Join(" ", arguments),
				PseudoConsole.PseudoConsoleThreadAttribute,
				_pseudoConsole.Handle);
			Running = true;

			// copy all pseudoconsole output to a FileStream and expose it to the rest of the app
			ConsoleOutStream = new FileStream(_outputPipe.ReadSide, FileAccess.Read);

			new Task(ReadOutputAsync).Start();

			// Store input pipe handle, and a writer for later reuse
			_consoleInputPipeWriteHandle = _inputPipe.WriteSide;
			_consoleInputWriter = new StreamWriter(new FileStream(_consoleInputPipeWriteHandle, FileAccess.Write))
			{
				AutoFlush = true
			};
			ForwardCtrlC(_consoleInputWriter);

			// free resources in case the console is ungracefully closed (e.g. by the 'x' in the window titlebar)
			OnClose(() => DisposeResources(_process, _pseudoConsole, _outputPipe, _inputPipe, _consoleInputWriter));

			WaitForExit(_process).WaitOne(Timeout.Infinite);
			Running = false;
			ProcessExited?.Invoke(this, EventArgs.Empty);
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

		private void DisposeResources(params IDisposable[] disposables)
		{
			foreach (var disposable in disposables)
			{
				disposable.Dispose();
			}
		}

		private async void ReadOutputAsync()
		{
			using var reader = new StreamReader(ConsoleOutStream);
			var standardOutput = new StringBuilder();
			var buffer = new char[4096];

			while (Running)
			{
				standardOutput.Clear();
				var length = await reader.ReadAsync(buffer, 0, buffer.Length);
				if (length > 0)
				{
					standardOutput.Append(buffer.SubArray(0, length));
					OnStandardTextReceived(standardOutput.ToString());
				}
			}
		}

		private void OnStandardTextReceived(string e)
		{
			var handler = OutputReady;
			handler?.Invoke(this, e);
		}

		/// <summary>
		///     Get an AutoResetEvent that signals when the process exits
		/// </summary>
		private static AutoResetEvent WaitForExit(Process process)
		{
			return new AutoResetEvent(false)
			{
				SafeWaitHandle = new SafeWaitHandle(process.ProcessInfo.hProcess, false)
			};
		}

		/// <summary>
		///     Don't let ctrl-c kill the terminal, it should be sent to the process in the terminal.
		/// </summary>
		private static void ForwardCtrlC(StreamWriter writer)
		{
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				writer.Write(CtrlCCommand);
			};
		}

		/// <summary>
		///     Set a callback for when the terminal is closed (e.g. via the "X" window decoration button).
		///     Intended for resource cleanup logic.
		/// </summary>
		private static void OnClose(Action handler)
		{
			SetConsoleCtrlHandler(eventType =>
			{
				if (eventType == CtrlTypes.CTRL_CLOSE_EVENT) handler();
				return false;
			}, true);
		}
	}
}
