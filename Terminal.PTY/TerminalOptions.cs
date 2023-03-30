using System.Collections.Generic;

namespace Terminal.PTY
{
	/// <summary>
	///     Provides configuration for Terminal
	/// </summary>
	public interface ITerminalOptions
	{
		/// <summary>
		///     PseudoConsole method.
		///     Choose between ConPtyTerminal, WinPtyTerminal or ProcessPtyTerminal.
		/// </summary>
		PseudoConsoleType PseudoConsoleType { get; set; }

		/// <summary>
		///     Console application executable. Should be the name of an exe file in the root directory.
		/// </summary>
		string Executable { get; set; }

		/// <summary>
		///     Root directory of the terminal executable.
		/// </summary>
		string Directory { get; set; }

		/// <summary>
		///     Default arguments for terminal on first launch
		/// </summary>
		string Arguments { get; set; }

		/// <summary>
		///     Gets or sets the terminals environment variables.
		/// </summary>
		IDictionary<string, string> EnvironmentVariables { get; set; }
	}

	/// <inheritdoc />
	public class TerminalOptions : ITerminalOptions
	{
		/// <summary>
		///     Name of key in appsettings.json.
		/// </summary>
		public const string Terminal = "Terminal";

		public TerminalOptions(string executable = null, string directory = null, string arguments = null, IDictionary<string, string> environmentVariables = null)
		{
			Executable = executable;
			Directory = directory;
			Arguments = arguments;
			EnvironmentVariables = environmentVariables;
		}

		/// <inheritdoc />
		public PseudoConsoleType PseudoConsoleType { get; set; }

		/// <inheritdoc />
		public string Executable { get; set; }

		/// <inheritdoc />
		public string Directory { get; set; }

		/// <inheritdoc />
		public string Arguments { get; set; }

		/// <inheritdoc />
		public IDictionary<string, string> EnvironmentVariables { get; set; }
	}
}
