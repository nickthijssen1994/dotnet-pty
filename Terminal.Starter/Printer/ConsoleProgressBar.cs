using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Terminal.Starter.Printer
{
	public class ConsoleProgressBar : IDisposable, IProgress<double>
	{
		private static int _startColumn;
		private static int _startRow;
		private static int _currentColumn;
		private static int _currentRow;
		private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);
		private readonly Timer _timer;
		private int _animationIndex;
		private double _currentProgress;
		private string _currentText = string.Empty;
		private bool _disposed;

		public ConsoleProgressBar()
		{
			Console.OutputEncoding = Encoding.UTF8;

			_startColumn = Console.CursorLeft;
			_startRow = Console.CursorTop;

			NumberOfBlocks = 10;
			StartBracket = "[";
			EndBracket = "]";
			CompletedBlock = "#";
			IncompleteBlock = "-";
			AnimationSequence = ProgressAnimations.Default;

			DisplayBar = true;
			DisplayPercentComplete = true;
			DisplayAnimation = true;

			_timer = new Timer(TimerHandler);

			// A progress bar is only for temporary display in a console window.
			// If the console output is redirected to a file, draw nothing.
			// Otherwise, we'll end up with a lot of garbage in the target file.
			if (!Console.IsOutputRedirected) ResetTimer();
		}

		public int NumberOfBlocks { get; set; }
		public string StartBracket { get; set; }
		public string EndBracket { get; set; }
		public string CompletedBlock { get; set; }
		public string IncompleteBlock { get; set; }
		public string AnimationSequence { get; set; }
		public bool DisplayBar { get; set; }
		public bool DisplayPercentComplete { get; set; }
		public bool DisplayAnimation { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Report(double value)
		{
			// Make sure value is in [0..1] range
			value = Math.Max(0, Math.Min(1, value));
			Interlocked.Exchange(ref _currentProgress, value);
		}

		private void TimerHandler(object state)
		{
			lock (_timer)
			{
				if (_disposed) return;
				UpdateText(GetProgressBarText(_currentProgress));
				ResetTimer();
			}
		}

		private string GetProgressBarText(double currentProgress)
		{
			const string singleSpace = " ";

			var numBlocksCompleted = (int)(currentProgress * NumberOfBlocks);

			var completedBlocks =
				Enumerable.Range(0, numBlocksCompleted).Aggregate(
					string.Empty,
					(current, _) => current + CompletedBlock);

			var incompleteBlocks =
				Enumerable.Range(0, NumberOfBlocks - numBlocksCompleted).Aggregate(
					string.Empty,
					(current, _) => current + IncompleteBlock);

			var progressBar = $"{StartBracket}{completedBlocks}{incompleteBlocks}{EndBracket}";
			var percent = $"{currentProgress:P0}".PadLeft(4, '\u00a0');
			var animationFrame = AnimationSequence[_animationIndex++ % AnimationSequence.Length];
			var animation = $"{animationFrame}";

			progressBar = DisplayBar
				? progressBar + singleSpace
				: string.Empty;

			percent = DisplayPercentComplete
				? percent + singleSpace
				: string.Empty;

			if (!DisplayAnimation || currentProgress is 1) animation = string.Empty;

			return (progressBar + percent + animation).TrimEnd();
		}

		private void UpdateText(string text)
		{
			_currentColumn = Console.CursorLeft;
			_currentRow = Console.CursorTop;
			Console.CursorLeft = _startColumn;
			Console.CursorTop = _startRow;
			// Get length of common portion
			var commonPrefixLength = 0;
			var commonLength = Math.Min(_currentText.Length, text.Length);
			while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength])
			{
				commonPrefixLength++;
			}

			// Backtrack to the first differing character
			var outputBuilder = new StringBuilder();
			outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);

			// Output new suffix
			outputBuilder.Append(text.Substring(commonPrefixLength));

			// If the new text is shorter than the old one: delete overlapping characters
			var overlapCount = _currentText.Length - text.Length;
			if (overlapCount > 0)
			{
				outputBuilder.Append(' ', overlapCount);
				outputBuilder.Append('\b', overlapCount);
			}

			//Console.Write($"{Caption}{outputBuilder}");
			Console.Write(outputBuilder);
			_currentText = text;
			Console.CursorLeft = _currentColumn;
			Console.CursorTop = _currentRow;
		}

		private void ResetTimer()
		{
			_timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;

			lock (_timer)
			{
				_disposed = true;
			}
		}
	}
}