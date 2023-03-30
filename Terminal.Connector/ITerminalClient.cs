using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Terminal.Connector
{
	public interface ITerminalClient
	{
		[HubMethodName("ReceiveTerminalOutput")]
		Task ReceiveTerminalOutput(string output);

		[HubMethodName("ReceiveCommandLineData")]
		Task ReceiveCommandLineData(string output);

		[HubMethodName("ReceiveTerminalExecutables")]
		Task ReceiveTerminalExecutables(string output);

		[HubMethodName("ReceiveUsername")]
		Task ReceiveUsername(string username);

		[HubMethodName("ReceiveTerminalReset")]
		Task ReceiveTerminalReset();
	}
}
