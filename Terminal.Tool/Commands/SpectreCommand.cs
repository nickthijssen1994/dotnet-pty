using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Terminal.Tool.Commands
{
	public class SpectreCommand : Command
	{
		public SpectreCommand() : base("spectre", "Show Spectre examples")
		{
			var showProgressBar = new Command("progressbar", "Show progressbar example");
			showProgressBar.Handler =
				CommandHandler.Create((CancellationToken token) => HandleProgressBarCommand(token));
			AddCommand(showProgressBar);
			var showCalendar = new Command("calendar", "Show calendar example");
			showCalendar.Handler =
				CommandHandler.Create((CancellationToken token) => HandleCalendarCommand(token));
			AddCommand(showCalendar);
			var showTree = new Command("tree", "Show tree example");
			showTree.Handler =
				CommandHandler.Create((CancellationToken token) => HandleTreeCommand(token));
			AddCommand(showTree);
			var showTable = new Command("table", "Show table example");
			showTable.Handler =
				CommandHandler.Create((CancellationToken token) => HandleTableCommand(token));
			AddCommand(showTable);
			AddAlias("-sp");
			Handler = CommandHandler.Create((CancellationToken token) => HandleProgressBarCommand(token));
		}

		private async Task<int> HandleCalendarCommand(CancellationToken token)
		{
			try
			{
				AnsiConsole.Render(new Calendar(2020, 10)
					.Border(TableBorder.Ascii)
					.HighlightStyle(Style.Parse("red"))
					.HeaderStyle(Style.Parse("yellow"))
					.AddCalendarEvent("An event", 2020, 9, 22)
					.AddCalendarEvent("Another event", 2020, 10, 2)
					.AddCalendarEvent("A third event", 2020, 10, 13));

				AnsiConsole.WriteLine();
				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}

		private async Task<int> HandleTreeCommand(CancellationToken token)
		{
			try
			{
				// Render the tree
				var tree = BuildTree();
				AnsiConsole.Render(tree);

				AnsiConsole.WriteLine();
				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}

		private async Task<int> HandleTableCommand(CancellationToken token)
		{
			try
			{
				AnsiConsole.Render(CreateTable());
				AnsiConsole.WriteLine();
				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}

		private async Task<int> HandleProgressBarCommand(CancellationToken token)
		{
			try
			{
				AnsiConsole.Console.Profile.Capabilities.Interactive = true;
				AnsiConsole.MarkupLine("[yellow]Initializing warp drive[/]...");

				await AnsiConsole.Create(new AnsiConsoleSettings
					{
						ColorSystem = ColorSystemSupport.TrueColor,
						Interactive = InteractionSupport.Yes
					})
					.Progress()
					.AutoClear(false)
					.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(),
						new RemainingTimeColumn(), new SpinnerColumn())
					.StartAsync(async ctx =>
					{
						var random = new Random(DateTime.Now.Millisecond);

						// Create some tasks
						var tasks = CreateTasks(ctx, random);
						var warpTask = ctx.AddTask("Going to warp", false).IsIndeterminate();

						// Wait for all tasks (except the indeterminate one) to complete
						while (!ctx.IsFinished)
						{
							// Increment progress
							foreach (var (task, increment) in tasks)
							{
								task.Increment(random.NextDouble() * increment);
							}

							// Simulate some delay
							await Task.Delay(20, token);
						}

						// Now start the "warp" task
						warpTask.StartTask();
						warpTask.IsIndeterminate(false);
						while (!ctx.IsFinished)
						{
							warpTask.Increment(12 * random.NextDouble());

							// Simulate some delay
							await Task.Delay(20, token);
						}
					});
				AnsiConsole.MarkupLine("[green]Done![/]");
				AnsiConsole.WriteLine();

				await Task.Delay(200, token);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Task Was Cancelled");
				return 1;
			}

			return 0;
		}

		private static List<(ProgressTask Task, int Delay)> CreateTasks(ProgressContext progress, Random random)
		{
			var tasks = new List<(ProgressTask, int)>();
			while (tasks.Count < 5)
			{
				if (DescriptionGenerator.TryGenerate(out var name))
					tasks.Add((progress.AddTask(name), random.Next(2, 10)));
			}

			return tasks;
		}

		private static Table CreateTable()
		{
			var simple = new Table()
				.Border(TableBorder.Ascii)
				.BorderColor(Color.Red)
				.AddColumn(new TableColumn("[u]CDE[/]").Footer("EDC").Centered())
				.AddColumn(new TableColumn("[u]FED[/]").Footer("DEF"))
				.AddColumn(new TableColumn("[u]IHG[/]").Footer("GHI"))
				.AddRow("Hello", "[red]World![/]", "")
				.AddRow("[blue]Bonjour[/]", "[white]le[/]", "[red]monde![/]")
				.AddRow("[blue]Hej[/]", "[yellow]Världen![/]", "");

			var second = new Table()
				.Border(TableBorder.Ascii)
				.BorderColor(Color.Green)
				.AddColumn(new TableColumn("[u]Foo[/]"))
				.AddColumn(new TableColumn("[u]Bar[/]"))
				.AddColumn(new TableColumn("[u]Baz[/]"))
				.AddRow("Hello", "[red]World![/]", "")
				.AddRow(simple, new Text("Whaaat"), new Text("Lolz"))
				.AddRow("[blue]Hej[/]", "[yellow]Världen![/]", "");

			return new Table()
				.Border(TableBorder.Ascii)
				.Title("TABLE [yellow]TITLE[/]")
				.Caption("TABLE [yellow]CAPTION[/]")
				.AddColumn(new TableColumn(new Panel("[u]ABC[/]").BorderColor(Color.Red)).Footer("[u]FOOTER 1[/]"))
				.AddColumn(new TableColumn(new Panel("[u]DEF[/]").BorderColor(Color.Green)).Footer("[u]FOOTER 2[/]"))
				.AddColumn(new TableColumn(new Panel("[u]GHI[/]").BorderColor(Color.Blue)).Footer("[u]FOOTER 3[/]"))
				.AddRow(new Text("Hello").Centered(), new Markup("[red]World![/]"), Text.Empty)
				.AddRow(second, new Text("Whaaat"), new Text("Lol"))
				.AddRow(new Markup("[blue]Hej[/]").Centered(), new Markup("[yellow]Världen![/]"), Text.Empty);
		}

		private static Tree BuildTree()
		{
			// Create the tree
			var tree = new Tree("Root")
				.Style(Style.Parse("red"))
				.Guide(TreeGuide.BoldLine);

			// Add some nodes
			var foo = tree.AddNode("[yellow]Foo[/]");
			var table = foo.AddNode(new Table()
				.AsciiBorder()
				.AddColumn("First")
				.AddColumn("Second")
				.AddRow("1", "2")
				.AddRow("3", "4")
				.AddRow("5", "6"));

			table.AddNode("[blue]Baz[/]");
			foo.AddNode("Qux");

			var bar = tree.AddNode("[yellow]Bar[/]");
			bar.AddNode(new Calendar(2020, 12)
				.AddCalendarEvent(2020, 12, 12)
				.HideHeader());

			// Return the tree
			return tree;
		}
	}
}