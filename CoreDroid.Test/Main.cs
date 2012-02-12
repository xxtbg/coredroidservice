using System;
using System.IO;
using System.Reflection;

namespace CoreDroid.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SocketClient client = new SocketClient (Convert.ToInt32 (Console.ReadLine ()));
			
			client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Test.Plugin.dll")));
			TestService service = client.GetService<TestService> ();
			string[] contents = service.List ("/");
			
			foreach (string content in contents)
				Console.WriteLine (content);
			
			service.Close ();
			client.Close ();
		}
	}
}
