namespace Terminal.Connector
{
	public interface ITerminalRepository
	{
		void SelectTerminal(string executable);
		void HandleStart(string executable, string arguments);
		void WriteToPseudoConsole(string executable, string input);
		string GetOutputBuffer(string executable);
		string GetCommandListData(string executable);
		string GetTerminalExecutables();
	}
}
