// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Terminal.PTY.WinPty
{
	/// <summary>
	///     Provides platform specific functionality.
	/// </summary>
	internal static class PlatformServices
	{
		private static readonly Lazy<IPtyProvider> WindowsProviderLazy = new(() => new Windows.PtyProvider());
		private static readonly Lazy<IPtyProvider> PtyProviderLazy;
		private static readonly IDictionary<string, string> WindowsPtyEnvironment = new Dictionary<string, string>();

		static PlatformServices()
		{
			if (IsWindows)
			{
				PtyProviderLazy = WindowsProviderLazy;
				EnvironmentVariableComparer = StringComparer.OrdinalIgnoreCase;
				PtyEnvironment = WindowsPtyEnvironment;
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
		}

		/// <summary>
		///     Gets the <see cref="IPtyProvider" /> for the current platform.
		/// </summary>
		public static IPtyProvider PtyProvider => PtyProviderLazy.Value;

		/// <summary>
		///     Gets the comparer to determine if two environment variable keys are equivalent on the current platform.
		/// </summary>
		public static StringComparer EnvironmentVariableComparer { get; }

		/// <summary>
		///     Gets specific environment variables that are needed when spawning the PTY.
		/// </summary>
		public static IDictionary<string, string> PtyEnvironment { get; }

		private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
	}
}