using Microsoft.AspNetCore.SignalR;

namespace Terminal.Connector
{
	public class NameUserIdProvider : IUserIdProvider
	{
		public string? GetUserId(HubConnectionContext connection)
		{
			return connection.User?.Identity?.Name;
		}
	}
}
