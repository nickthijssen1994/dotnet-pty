﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace Terminal.PTY.WinPty.Windows
{
	using static NativeMethods;

	/// <summary>
	///     A connection to a pseudoterminal spawned by native windows APIs.
	/// </summary>
	internal sealed class PseudoConsoleConnection : IPtyConnection
	{
		private readonly PseudoConsoleConnectionHandles handles;
		private readonly Process process;

		/// <summary>
		///     Initializes a new instance of the <see cref="PseudoConsoleConnection" /> class.
		/// </summary>
		/// <param name="handles">The set of handles associated with the pseudoconsole.</param>
		public PseudoConsoleConnection(PseudoConsoleConnectionHandles handles)
		{
			ReaderStream = new AnonymousPipeClientStream(PipeDirection.In,
				new Microsoft.Win32.SafeHandles.SafePipeHandle(handles.OutPipeOurSide.Handle, false));
			WriterStream = new AnonymousPipeClientStream(PipeDirection.Out,
				new Microsoft.Win32.SafeHandles.SafePipeHandle(handles.InPipeOurSide.Handle, false));

			this.handles = handles;
			process = Process.GetProcessById(Pid);
			process.Exited += Process_Exited;
			process.EnableRaisingEvents = true;
		}

		/// <inheritdoc />
		public event EventHandler<PtyExitedEventArgs>? ProcessExited;

		/// <inheritdoc />
		public Stream ReaderStream { get; }

		/// <inheritdoc />
		public Stream WriterStream { get; }

		/// <inheritdoc />
		public int Pid => handles.Pid;

		/// <inheritdoc />
		public int ExitCode => process.ExitCode;

		/// <inheritdoc />
		public void Dispose()
		{
			ReaderStream?.Dispose();
			WriterStream?.Dispose();

			if (handles != null)
			{
				handles.PseudoConsoleHandle.Close();
				handles.MainThreadHandle.Close();
				handles.ProcessHandle.Close();
				handles.InPipeOurSide.Close();
				handles.InPipePseudoConsoleSide.Close();
				handles.OutPipePseudoConsoleSide.Close();
				handles.OutPipeOurSide.Close();
			}
		}

		/// <inheritdoc />
		public void Kill()
		{
			process.Kill();
		}

		/// <inheritdoc />
		public void Resize(int cols, int rows)
		{
			var hr = ResizePseudoConsole(handles.PseudoConsoleHandle, new Coord(cols, rows));
			if (hr != S_OK) Marshal.ThrowExceptionForHR(hr);
		}

		/// <inheritdoc />
		public bool WaitForExit(int milliseconds)
		{
			return process.WaitForExit(milliseconds);
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			ProcessExited?.Invoke(this, new PtyExitedEventArgs(process.ExitCode));
		}

		/// <summary>
		///     handles to resources creates when a pseudoconsole is spawned.
		/// </summary>
		internal sealed class PseudoConsoleConnectionHandles
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="PseudoConsoleConnectionHandles" /> class.
			/// </summary>
			/// <param name="inPipePseudoConsoleSide">the input pipe on the pseudoconsole side.</param>
			/// <param name="outPipePseudoConsoleSide">the output pipe on the pseudoconsole side.</param>
			/// <param name="inPipeOurSide"> the input pipe on the local side.</param>
			/// <param name="outPipeOurSide"> the output pipe on the local side.</param>
			/// <param name="pseudoConsoleHandle">the handle to the pseudoconsole.</param>
			/// <param name="processHandle">the handle to the spawned process.</param>
			/// <param name="pid">the process ID.</param>
			/// <param name="mainThreadHandle">the handle to the main thread.</param>
			public PseudoConsoleConnectionHandles(
				SafePipeHandle inPipePseudoConsoleSide,
				SafePipeHandle outPipePseudoConsoleSide,
				SafePipeHandle inPipeOurSide,
				SafePipeHandle outPipeOurSide,
				SafePseudoConsoleHandle pseudoConsoleHandle,
				SafeProcessHandle processHandle,
				int pid,
				SafeThreadHandle mainThreadHandle)
			{
				InPipePseudoConsoleSide = inPipePseudoConsoleSide;
				OutPipePseudoConsoleSide = outPipePseudoConsoleSide;
				InPipeOurSide = inPipeOurSide;
				OutPipeOurSide = outPipeOurSide;
				PseudoConsoleHandle = pseudoConsoleHandle;
				ProcessHandle = processHandle;
				Pid = pid;
				MainThreadHandle = mainThreadHandle;
			}

			/// <summary>
			///     Gets the input pipe on the pseudoconsole side.
			/// </summary>
			/// <remarks>
			///     This pipe is connected to <see cref="OutPipeOurSide" />.
			/// </remarks>
			internal SafePipeHandle InPipePseudoConsoleSide { get; }

			/// <summary>
			///     Gets the output pipe on the pseudoconsole side.
			/// </summary>
			/// <remarks>
			///     This pipe is connected to <see cref="InPipeOurSide" />.
			/// </remarks>
			internal SafePipeHandle OutPipePseudoConsoleSide { get; }

			/// <summary>
			///     Gets the input pipe on the local side.
			/// </summary>
			/// <remarks>
			///     This pipe is connected to <see cref="OutPipePseudoConsoleSide" />.
			/// </remarks>
			internal SafePipeHandle InPipeOurSide { get; }

			/// <summary>
			///     Gets the output pipe on the local side.
			/// </summary>
			/// <remarks>
			///     This pipe is connected to <see cref="InPipePseudoConsoleSide" />.
			/// </remarks>
			internal SafePipeHandle OutPipeOurSide { get; }

			/// <summary>
			///     Gets the handle to the pseudoconsole.
			/// </summary>
			internal SafePseudoConsoleHandle PseudoConsoleHandle { get; }

			/// <summary>
			///     Gets the handle to the spawned process.
			/// </summary>
			internal SafeProcessHandle ProcessHandle { get; }

			/// <summary>
			///     Gets the process ID.
			/// </summary>
			internal int Pid { get; }

			/// <summary>
			///     Gets the handle to the main thread.
			/// </summary>
			internal SafeThreadHandle MainThreadHandle { get; }
		}
	}
}