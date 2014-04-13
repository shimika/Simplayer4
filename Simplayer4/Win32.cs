using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Simplayer4 {
	internal static class Win32 {
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("kernel32", SetLastError = true)]
		public static extern short GlobalAddAtom(string lpString);

		[DllImport("kernel32", SetLastError = true)]
		public static extern short GlobalDeleteAtom(short nAtom);

		public const int MOD_ALT = 1;
		public const int MOD_CONTROL = 2;
		public const int MOD_SHIFT = 4;
		public const int MOD_WIN = 8;

		public const uint VK_KEY_C = 0x43;
		public const uint VK_KEY_W = 0x57;
		public const uint VK_DOWN = 0x28;
		public const uint VK_UP = 0x26;
		public const uint VK_LEFT = 0x25;
		public const uint VK_RIGHT = 0x27;
		public const uint VK_KEY_D = 0x44;
		public const uint VK_OEM_COMMA = 0xBC;
		public const uint VK_OEM_PERIOD = 0xBE;

		public const int WM_HOTKEY = 0x312;
	}
}
