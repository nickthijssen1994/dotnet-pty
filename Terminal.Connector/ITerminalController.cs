using System.Threading.Tasks;

namespace Terminal.Connector
{
	public interface ITerminalController
	{
		Task SelectTerminal(string input);
		Task HandleInput(string executable, string input);
		Task HandleOutput(string executable, string output);
		Task HandleStart(string executable, string arguments);
		Task HandleTerminalReset(string executable);
		string GetOutputBuffer(string executable);
		string GetCommandListData(string executable);
		string GetTerminalExecutables();
	}
}
