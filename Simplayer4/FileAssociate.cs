using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace Simplayer4 {
	public class FileAssociate {
		private const string SUFFIX = "mp3";      // the suffix you want to associate with this program
		private const string SHORT_DESC = "Simplayer.mp3";  // a one word name for the file type
		private const string DSUFFIX = "." + SUFFIX;

		[DllImport("Shell32.dll")]
		private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

		public static void Associate() {
			string exename, exepath, execommand;
			exename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			exepath = AppDomain.CurrentDomain.BaseDirectory;
			execommand = "\"" + exepath + exename + "\" \"%1\"";

			RegistryKey r, r2;
			string[] ss;

			r = Registry.ClassesRoot;
			ss = r.GetSubKeyNames();

			if (Array.IndexOf<string>(ss, DSUFFIX) >= 0) r.DeleteSubKeyTree(DSUFFIX);
			r.CreateSubKey(DSUFFIX);
			r2 = r.OpenSubKey(DSUFFIX, true);
			r2.SetValue("", SHORT_DESC);    // the default value
			r2.Close();
			/*
			r.CreateSubKey(SHORT_DESC + @"\DefaultIcon");
			r2 = r.OpenSubKey(SHORT_DESC + @"\DefaultIcon", true);
			r2.SetValue("", @"C:\Users\Administrator\AppData\Roaming\Simplayer4.ico");
			r2.Close();
			 */ 

			r.CreateSubKey(SHORT_DESC + @"\shell\open\command");
			r2 = r.OpenSubKey(SHORT_DESC + @"\shell\open\command", true);
			r2.SetValue("", execommand);
			r2.Close();

			r.CreateSubKey(@"Applications\" + exename + @"\shell\open\command");
			r2 = r.OpenSubKey(@"Applications\" + exename + @"\shell\open\command", true);
			r2.SetValue("", execommand);
			r2.Close();


			SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
		}
	}
}
