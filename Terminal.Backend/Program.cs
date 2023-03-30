using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Terminal.Backend
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureLogging(configureLogging =>
				{
					configureLogging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information);
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<EventLogSettings>(config =>
					{
						config.LogName = "Terminal.Service";
						config.SourceName = "Terminal.Service Source";
					});
				})
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		}
	}
}