using System;
using Microsoft.Win32.SafeHandles;
using static Terminal.PTY.ConPty.Native.PseudoConsoleApi;

namespace Terminal.PTY.ConPty
{
	/// <summary>
	///     Utility functions around the new Pseudo Console APIs
	/// </summary>
	public sealed class PseudoConsole : IDisposable
	{
		public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;

		private PseudoConsole(IntPtr handle)
		{
			Handle = handle;
		}

		public IntPtr Handle { get; }

		public void Dispose()
		{
			ClosePseudoConsole(Handle);
		}

		public static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width,
			int height)
		{
			var createResult = CreatePseudoConsole(
				new COORD { X = (short)width, Y = (short)height },
				inputReadSide, outputWriteSide,
				0, out var hPC);
			if (createResult != 0)
				throw new InvalidOperationException("Could not create psuedo console. Error Code " + createResult);
			return new PseudoConsole(hPC);
		}
	}
}