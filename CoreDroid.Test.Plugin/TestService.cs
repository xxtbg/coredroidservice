using System;
using System.Linq;
using Mono.Unix;
using CoreDroid.Contract;
using Mono.Unix.Native;

namespace CoreDroid.Test
{
	[ServiceContract]
	public class TestService
	{
		public string[] List (string path)
		{
			UnixDirectoryInfo dirInfo = new UnixDirectoryInfo (path);
			UnixFileSystemInfo[] entries = dirInfo.GetFileSystemEntries ();
			
			return entries.Select (e => e.FullName).ToArray ();
		}
	}
}

