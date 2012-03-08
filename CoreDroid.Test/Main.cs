using System;
using System.IO;
using System.Reflection;
using CoreDroid.Contract;
using DiskDroid.FileSystem;
using DiskDroid.FileSystem.Contract;

namespace CoreDroid.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				SocketClient client = new SocketClient (10000);//Convert.ToInt32 (Console.ReadLine ()));
			
				client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Test.Contract.dll")));
				client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Test.Plugin.dll")));
				DirectoryService service = client.GetService<DirectoryService> ();
				DirectoryItemInfo directory = service.Get ("/") as DirectoryItemInfo;
				FileSystemItemInfo[] contents = service.GetContents (directory);
			
				foreach (FileSystemItemInfo content in contents)
					Console.WriteLine (content.Path);
			
				service.Close ();
				client.Close ();
			} catch (ServiceException ex) {
				Console.WriteLine ("ServiceException");
				Console.WriteLine ("\tAssemblyName: " + ex.OriginalAssemblyName);
				Console.WriteLine ("\tTypeName: " + ex.OriginalTypeName);
				Console.WriteLine ("\tMessage: " + ex.OriginalMessage);
				Console.WriteLine ("\tStackTrace: " + ex.OriginalStackTrace);
			}
		}
	}
}
