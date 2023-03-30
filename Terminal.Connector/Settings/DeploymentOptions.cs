namespace Terminal.Connector.Settings
{
	public class DeploymentOptions
	{
		public const string Deployment = "Deployment";

		/// <summary>
		///     Active Directory Group were users need to be part of to access functions that are restricted by group.
		/// </summary>
		public string TerminalUserGroup { get; set; }

		/// <summary>
		///     Endpoint route for SignalR terminal hub.
		///     For example "/terminal".
		/// </summary>
		public string TerminalHubEndpoint { get; set; }
	}
}
