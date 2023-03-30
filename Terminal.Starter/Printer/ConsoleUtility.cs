using System;

namespace Terminal.Starter.Printer
{
	public static class ConsoleUtility
	{
		private const char _block = '■';
		private const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
		private const string _twirl = "-\\|/";
		private static int startColumn;
		private static int startRow;
		private static int currentColumn;
		private static int currentRow;
		private static readonly int totalChunks = 30;

		public static void WriteProgressBar(int progress, int tot, bool update = false)
		{
			currentColumn = Console.CursorLeft;
			currentRow = Console.CursorTop;
			if (!update)
			{
				startColumn = Console.CursorLeft;
				startRow = Console.CursorTop;
			}
			else
			{
				Console.CursorLeft = startColumn;
				Console.CursorTop = startRow;
			}

			Console.CursorTop = startRow;
			Console.CursorVisible = false;
			//draw empty progress bar
			Console.CursorLeft = 0;
			Console.Write("["); //start
			Console.CursorLeft = totalChunks + 1;
			Console.Write("]"); //end
			Console.CursorLeft = 1;

			var onechunk = 30.0f / tot;

			//draw filled part
			var position = 1;
			for (var i = 0; i < onechunk * progress; i++)
			{
				Console.BackgroundColor = ConsoleColor.Green;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw unfilled part
			for (var i = position; i <= totalChunks; i++)
			{
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			Console.BackgroundColor = ConsoleColor.Black;

			Console.CursorLeft = currentColumn;
			Console.CursorTop = currentRow;
		}

		public static void WriteProgressBarPercentage(int percent, bool update = false)
		{
			if (update)
				Console.Write(_back);
			Console.Write("[");
			var p = (int)(percent / 10f + .5f);
			for (var i = 0; i < 10; ++i)
			{
				if (i >= p)
					Console.Write(' ');
				else
					Console.Write(_block);
			}

			Console.Write("] {0,3:##0}%", percent);
		}

		public static void WriteProgress(int progress, bool update = false)
		{
			if (update)
				Console.Write("\b");
			Console.Write(_twirl[progress % _twirl.Length]);
		}
	}
}