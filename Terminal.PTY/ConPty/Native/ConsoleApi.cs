﻿using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Terminal.PTY.ConPty.Native
{
	/// <summary>
	///     PInvoke signatures for win32 console api
	/// </summary>
	public static class ConsoleApi
	{
		public delegate bool ConsoleEventDelegate(CtrlTypes ctrlType);

		public enum CtrlTypes : uint
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		public const int STD_OUTPUT_HANDLE = -11;
		public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
		public const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern SafeFileHandle GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetConsoleMode(SafeFileHandle hConsoleHandle, uint mode);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetConsoleMode(SafeFileHandle handle, out uint mode);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
	}
}