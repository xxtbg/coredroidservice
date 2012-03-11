using System;
using System.Diagnostics;

namespace CoreDroid.Extensions
{
	public static class StringExtensions
	{
		public static string Run (this string command, params string[] arguments)
		{
			for (int i = 0; i < arguments.Length; i++)
				arguments [i] = string.Concat ("\"", arguments [i], "\"");
			
			// System.IO.Path.Combine (System.IO.Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "stat")
			Process proc = new Process ();
			proc.EnableRaisingEvents = false;
			proc.StartInfo.FileName = command;
			proc.StartInfo.Arguments = string.Join (" ", arguments);
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start ();
			proc.WaitForExit ();
			
			string error = proc.StandardError.ReadToEnd ();
			
			if (!string.IsNullOrEmpty (error)) {
				throw(new Exception (error));
			}
			
			return proc.StandardOutput.ReadToEnd ();
		}
	}
}

