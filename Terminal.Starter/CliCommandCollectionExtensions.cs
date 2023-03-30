using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Terminal.Starter
{
	/// <summary>
	///     Contains the collection extensions for adding the CLI commands.
	/// </summary>
	public static class CliCommandCollectionExtensions
	{
		public static IServiceCollection AddDefaultCommands(this IServiceCollection services)
		{
			return services.AddAssemblyCommands(Assembly.GetExecutingAssembly());
		}

		public static IServiceCollection AddProjectCommands(this IServiceCollection services)
		{
			return services.AddAssemblyCommands(Assembly.GetEntryAssembly() ?? throw new InvalidOperationException());
		}

		public static IServiceCollection AddAssembliesCommands(this IServiceCollection services,
			params Assembly[] assemblies)
		{
			foreach (var assembly in assemblies)
			{
				services.AddAssemblyCommands(assembly);
			}

			return services;
		}

		public static IServiceCollection AddAssemblyCommands(this IServiceCollection services,
			Assembly assembly)
		{
			try
			{
				var registrations = assembly.GetTypes()
					.Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface)
					.Where(type => !typeof(RootCommand).IsAssignableFrom(type))
					.Where(type => typeof(Command).IsAssignableFrom(type))
					.ToList();

				foreach (var registration in registrations)
				{
					services.AddCommand(registration);
				}
			}
			catch (Exception e)
			{
				if (e is ReflectionTypeLoadException loadException)
				{
					var messages = loadException.LoaderExceptions.Select(loaderException => loaderException.Message);
					throw new Exception(string.Join(";", messages), loadException);
				}

				throw;
			}

			return services;
		}

		public static IServiceCollection AddCommand(this IServiceCollection services, Type command)
		{
			try
			{
				if (command == typeof(RootCommand)) { }
				else
				{
					services.AddSingleton(typeof(Command), command);
				}
			}
			catch (Exception e)
			{
				if (e is ReflectionTypeLoadException loadException)
				{
					var messages = loadException.LoaderExceptions.Select(loaderException => loaderException.Message);
					throw new Exception(string.Join(";", messages), loadException);
				}

				throw;
			}

			return services;
		}

		public static TerminalShell BuildTerminal(this ServiceProvider provider, string? name = null,
			string? description = null)
		{
			return new TerminalShell(provider, name, description);
		}

		public static TerminalShell BuildTerminal(this ServiceProvider provider, RootCommand rootCommand)
		{
			return new TerminalShell(provider, rootCommand);
		}
	}
}