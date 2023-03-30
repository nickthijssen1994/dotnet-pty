using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Connector.Settings;
using Terminal.PTY;

namespace Terminal.Connector
{
	public static class DependencyInjectionExtensions
	{
		public static IServiceCollection AddTerminalRunner(this IServiceCollection services, ITerminalOptions options)
		{
			services.AddSingleton<ITerminalRepository, TerminalRepository>();

			services.AddSingleton(options);

			return services;
		}

		public static IServiceCollection AddKestrelSignalRTerminalEndpoint(this IServiceCollection services,
			DeploymentOptions deploymentOptions)
		{
			string ipAddress = Dns.GetHostEntry(Dns.GetHostName())
				.AddressList
				.First(x => x.AddressFamily == AddressFamily.InterNetwork)
				.ToString();

			services.AddCors(options =>
			{
				options.AddPolicy("SignalRPolicy", builder =>
				{
					builder
						.SetIsOriginAllowed(origin =>
							new Uri(origin).IsLoopback || new Uri(origin).Host == ipAddress)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

			services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
				.AddNegotiate();
			services.AddAuthorization(options =>
			{
				options.AddPolicy("TerminalPolicy", policy =>
				{
					policy.AuthenticationSchemes.Add(NegotiateDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					policy.RequireRole(deploymentOptions.TerminalUserGroup);
				});
			});
			services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
			services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; }).AddMessagePackProtocol();

			services.AddSingleton<ITerminalController, SignalRTerminalController>();

			return services;
		}
	}
}
