using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Terminal.Tool.Commands
{
	public class DownloadCommand : Command
	{
		public DownloadCommand() : base("download", "Run download task")
		{
			var url = new Option<string>("--url")
			{
				Name = "url",
				Description = "Url of file to download",
				IsRequired = false
			};

			var destination = new Option<string>("--destination")
			{
				Name = "destination",
				Description = "Location were file should be saved",
				IsRequired = false
			};

			AddOption(url);
			AddOption(destination);
			AddAlias("-d");
			Handler = CommandHandler.Create((string url, string destination, CancellationToken token) =>
				HandleCommand(url, destination, token));
		}

		private async Task<int> HandleCommand(string url, string destination, CancellationToken token)
		{
			try
			{
				AnsiConsole.Console.Profile.Capabilities.Interactive = true;

				if (string.IsNullOrWhiteSpace(url))
				{
					var choose = AnsiConsole.Prompt(
						new TextPrompt<string>("Do you want to download an example file or from a given URL?")
							.InvalidChoiceMessage("[red]That's not a valid option[/]")
							.DefaultValue("Example")
							.AddChoice("From URL"));

					if (choose.Equals("From URL"))
					{
						var name = AnsiConsole.Ask<string>("URL:");
						url = name;
					}
					else
					{
						url =
							"https://upload.wikimedia.org/wikipedia/commons/archive/5/56/20091114180013%21The_Sounds_of_Earth_Record_Cover_-_GPN-2000-001978.jpg";
					}
				}

				if (string.IsNullOrWhiteSpace(destination))
				{
					if (AnsiConsole.Confirm("Save to download folder?"))
					{
						var userDownloadsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
						                             "\\Downloads";
						destination = Path.Combine(userDownloadsDirectory, "golden-record.jpg");
					}
					else
					{
						var directory = AnsiConsole.Ask<string>("Directory:");
						await Task.Delay(1000, token);
						var filename = AnsiConsole.Ask<string>("File name:");
						destination = Path.Combine(directory, filename);
					}
				}

				var downloadFileUrl = url;
				var destinationFilePath = destination;

				AnsiConsole.MarkupLine("[yellow]Starting Download[/]");
				AnsiConsole.MarkupLine($"[yellow]Downloading[/]: {url}");
				AnsiConsole.MarkupLine($"[yellow]Downloading to[/]: {destination}");

				var downloadWithProgress =
					new HttpClientDownloadWithProgress(downloadFileUrl, destinationFilePath);

				await AnsiConsole.Progress()
					.AutoClear(false)
					.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(),
						new TransferSpeedColumn(), new DownloadedColumn())
					.StartAsync(async ctx =>
					{
						var download = ctx.AddTask("[green]Downloading File[/]", false).IsIndeterminate();
						downloadWithProgress.ProgressChanged +=
							(totalFileSize, totalBytesDownloaded, progressPercentage) =>
							{
								download.MaxValue((double)totalFileSize);
								download.Value(totalBytesDownloaded);
							};

						download.StartTask();
						download.IsIndeterminate(false);
						var isStarted = false;
						while (!ctx.IsFinished)
						{
							if (!isStarted)
							{
								await Task.Run(async () => { await downloadWithProgress.StartDownload(); }, token);
								isStarted = true;
							}

							await Task.Delay(200, token);
						}
					});
				AnsiConsole.MarkupLine($"[yellow]Saved in[/]: {destination}");
				AnsiConsole.MarkupLine("[yellow]Finished Download[/]");
				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				AnsiConsole.WriteException(ex,
					ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
					ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}
	}
}