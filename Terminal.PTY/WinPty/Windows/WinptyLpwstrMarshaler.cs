// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace Terminal.PTY.WinPty.Windows
{
	/// <summary>
	///     Marshals a LPWStr (wchar_t *) to a string without destroying the LPWStr, this is needed by winpty.
	/// </summary>
	internal class WinptyLpwstrMarshaler : ICustomMarshaler
	{
		private static readonly ICustomMarshaler instance = new WinptyLpwstrMarshaler();

		/// <inheritdoc />
		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			return Marshal.PtrToStringUni(pNativeData);
		}

		/// <inheritdoc />
		public void CleanUpNativeData(IntPtr pNativeData) { }

		/// <inheritdoc />
		public int GetNativeDataSize()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc />
		public IntPtr MarshalManagedToNative(object ManagedObj)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc />
		public void CleanUpManagedData(object ManagedObj)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		///     Required method on <see cref="ICustomMarshaler" /> on order to work with native methods.
		/// </summary>
		/// <param name="cookie">passed in cookie token.</param>
		/// <returns>The static instance of this <see cref="WinptyLpwstrMarshaler" />.</returns>
		public static ICustomMarshaler GetInstance(string cookie)
		{
			return instance;
		}
	}
}