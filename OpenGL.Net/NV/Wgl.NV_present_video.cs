
// Copyright (C) 2015-2016 Luca Piccioni
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
// USA

#pragma warning disable 649, 1572, 1573

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OpenGL
{
	public partial class Wgl
	{
		/// <summary>
		/// Value of WGL_NUM_VIDEO_SLOTS_NV symbol.
		/// </summary>
		[RequiredByFeature("WGL_NV_present_video")]
		public const int NUM_VIDEO_SLOTS_NV = 0x20F0;

		/// <summary>
		/// Binding for wglEnumerateVideoDevicesNV.
		/// </summary>
		/// <param name="hDC">
		/// A <see cref="T:IntPtr"/>.
		/// </param>
		/// <param name="phDeviceList">
		/// A <see cref="T:IntPtr[]"/>.
		/// </param>
		[RequiredByFeature("WGL_NV_present_video")]
		public static int EnumerateVideoDevicesNV(IntPtr hDC, IntPtr[] phDeviceList)
		{
			int retValue;

			unsafe {
				fixed (IntPtr* p_phDeviceList = phDeviceList)
				{
					Debug.Assert(Delegates.pwglEnumerateVideoDevicesNV != null, "pwglEnumerateVideoDevicesNV not implemented");
					retValue = Delegates.pwglEnumerateVideoDevicesNV(hDC, p_phDeviceList);
					LogFunction("wglEnumerateVideoDevicesNV(0x{0}, {1}) = {2}", hDC.ToString("X8"), LogValue(phDeviceList), retValue);
				}
			}
			DebugCheckErrors(retValue);

			return (retValue);
		}

		/// <summary>
		/// Binding for wglBindVideoDeviceNV.
		/// </summary>
		/// <param name="hDC">
		/// A <see cref="T:IntPtr"/>.
		/// </param>
		/// <param name="uVideoSlot">
		/// A <see cref="T:UInt32"/>.
		/// </param>
		/// <param name="hVideoDevice">
		/// A <see cref="T:IntPtr"/>.
		/// </param>
		/// <param name="piAttribList">
		/// A <see cref="T:int[]"/>.
		/// </param>
		[RequiredByFeature("WGL_NV_present_video")]
		public static bool BindVideoDeviceNV(IntPtr hDC, UInt32 uVideoSlot, IntPtr hVideoDevice, int[] piAttribList)
		{
			bool retValue;

			unsafe {
				fixed (int* p_piAttribList = piAttribList)
				{
					Debug.Assert(Delegates.pwglBindVideoDeviceNV != null, "pwglBindVideoDeviceNV not implemented");
					retValue = Delegates.pwglBindVideoDeviceNV(hDC, uVideoSlot, hVideoDevice, p_piAttribList);
					LogFunction("wglBindVideoDeviceNV(0x{0}, {1}, 0x{2}, {3}) = {4}", hDC.ToString("X8"), uVideoSlot, hVideoDevice.ToString("X8"), LogValue(piAttribList), retValue);
				}
			}
			DebugCheckErrors(retValue);

			return (retValue);
		}

		/// <summary>
		/// Binding for wglQueryCurrentContextNV.
		/// </summary>
		/// <param name="iAttribute">
		/// A <see cref="T:int"/>.
		/// </param>
		/// <param name="piValue">
		/// A <see cref="T:int[]"/>.
		/// </param>
		[RequiredByFeature("WGL_NV_present_video")]
		public static bool QueryCurrentContextNV(int iAttribute, int[] piValue)
		{
			bool retValue;

			unsafe {
				fixed (int* p_piValue = piValue)
				{
					Debug.Assert(Delegates.pwglQueryCurrentContextNV != null, "pwglQueryCurrentContextNV not implemented");
					retValue = Delegates.pwglQueryCurrentContextNV(iAttribute, p_piValue);
					LogFunction("wglQueryCurrentContextNV({0}, {1}) = {2}", iAttribute, LogValue(piValue), retValue);
				}
			}
			DebugCheckErrors(retValue);

			return (retValue);
		}

		public unsafe static partial class UnsafeNativeMethods
		{
			[SuppressUnmanagedCodeSecurity()]
			[DllImport(Library, EntryPoint = "wglEnumerateVideoDevicesNV", ExactSpelling = true, SetLastError = true)]
			internal extern static unsafe int wglEnumerateVideoDevicesNV(IntPtr hDC, IntPtr* phDeviceList);

			[SuppressUnmanagedCodeSecurity()]
			[DllImport(Library, EntryPoint = "wglBindVideoDeviceNV", ExactSpelling = true, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal extern static unsafe bool wglBindVideoDeviceNV(IntPtr hDC, UInt32 uVideoSlot, IntPtr hVideoDevice, int* piAttribList);

			[SuppressUnmanagedCodeSecurity()]
			[DllImport(Library, EntryPoint = "wglQueryCurrentContextNV", ExactSpelling = true, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal extern static unsafe bool wglQueryCurrentContextNV(int iAttribute, int* piValue);

		}

		internal unsafe static partial class Delegates
		{
			[SuppressUnmanagedCodeSecurity()]
			internal unsafe delegate int wglEnumerateVideoDevicesNV(IntPtr hDC, IntPtr* phDeviceList);

			[ThreadStatic]
			internal static wglEnumerateVideoDevicesNV pwglEnumerateVideoDevicesNV;

			[SuppressUnmanagedCodeSecurity()]
			internal unsafe delegate bool wglBindVideoDeviceNV(IntPtr hDC, UInt32 uVideoSlot, IntPtr hVideoDevice, int* piAttribList);

			[ThreadStatic]
			internal static wglBindVideoDeviceNV pwglBindVideoDeviceNV;

			[SuppressUnmanagedCodeSecurity()]
			internal unsafe delegate bool wglQueryCurrentContextNV(int iAttribute, int* piValue);

			[ThreadStatic]
			internal static wglQueryCurrentContextNV pwglQueryCurrentContextNV;

		}
	}

}
