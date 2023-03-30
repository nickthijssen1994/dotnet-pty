using System;
using System.Threading;

namespace Terminal.PTY
{
	/// <summary>
	///     Interface for terminal implementations.
	/// </summary>
	public interface ITerminal
	{
		ITerminalOptions Options { get; }

		/// <summary>
		///     Boolean indicating if the terminal is running.
		/// </summary>
		bool Running { get; }

		/// <summary>
		///     Start the pseudoconsole and run the process as shown in
		///     https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#creating-the-pseudoconsole
		/// </summary>
		/// <param name="executable">the command to run, e.g. cmd.exe</param>
		/// <param name="arguments"></param>
		/// <param name="cancellationToken"></param>
		void ExecuteAsync(string executable, string[] arguments, CancellationToken cancellationToken);

		/// <summary>
		///     Fired once the console has been hooked up and has new output.
		/// </summary>
		event EventHandler<string> OutputReady;

		/// <summary>
		///     Fired once the console has been hooked up and has new error output.
		/// </summary>
		event EventHandler<string> ErrorOutputReady;

		/// <summary>
		///     Fired once the console has exited.
		/// </summary>
		event EventHandler ProcessExited;

		/// <summary>
		///     Send input to the console application.
		///     This can be used to give input for methods like ReadLine() or ReadKey().
		/// </summary>
		/// <remarks>
		///     It depends on the underlying method of the console application if input is processed immediately.
		///     If for example the underlying method is ReadLine(), then this method can be called multiple times.
		///     In this case input will be appended to previous input.
		///     Only when the string representation of Enter ("\r\n") is given as input, ReadLine() will be executed.
		///     In the case of ReadKey(), only a single character or a string representation of a ConsoleKey can be entered after
		///     which input is processed immediately.
		/// </remarks>
		/// <param name="input">the input to write to the console</param>
		void Write(string input);
	}
}
