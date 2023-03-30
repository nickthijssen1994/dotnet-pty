namespace Terminal.PTY
{
	/// <summary>
	///     Pseudoconsole types
	/// </summary>
	public enum PseudoConsoleType
	{
		/// <summary>
		///     Terminal using conpty.
		/// </summary>
		ConPtyTerminal,

		/// <summary>
		///     Terminal using winpty.
		/// </summary>
		WinPtyTerminal
	}
}
