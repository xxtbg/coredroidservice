using System;
using System.IO;
using System.Reflection;
using CoreDroid.Contract;

namespace CoreDroid.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				SocketClient client = new SocketClient (Convert.ToInt32 (Console.ReadLine ()));
			
				client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Test.Plugin.dll")));
				TestService service = client.GetService<TestService> ();
				string[] contents = service.List ("/");
			
				foreach (string content in contents)
					Console.WriteLine (content);
			
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
