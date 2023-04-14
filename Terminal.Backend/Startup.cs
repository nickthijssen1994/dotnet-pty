using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Terminal.Connector;
using Terminal.Connector.Settings;
using Terminal.PTY;

namespace Terminal.Backend
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var deploymentOptions = new DeploymentOptions();
			Configuration.GetSection(DeploymentOptions.Deployment).Bind(deploymentOptions);
			var terminalOptions = new TerminalOptions();
			Configuration.GetSection(TerminalOptions.Terminal).Bind(terminalOptions);

			services.AddKestrelSignalRTerminalEndpoint(deploymentOptions);

			services.AddTerminalRunner(terminalOptions);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
		{
			if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors("SignalRPolicy");

			app.UseAuthentication();
		app.UseAuthorization();

			var deploymentOptions = new DeploymentOptions();
			Configuration.GetSection(DeploymentOptions.Deployment).Bind(deploymentOptions);

			app.UseEndpoints(endpoints => { endpoints.MapHub<TerminalHub>(deploymentOptions.TerminalHubEndpoint); });
		}
	}
}
