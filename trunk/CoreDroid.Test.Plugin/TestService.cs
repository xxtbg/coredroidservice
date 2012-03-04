using System;
using System.Linq;
using System.IO;
using CoreDroid.Contract;
using System.Collections.Generic;

namespace CoreDroid.Test
{
	[ServiceContract]
	public class TestService
	{
		[ServiceMember]
		public string[] List (string path)
		{
			List<string > directories = Directory.GetDirectories (path).ToList ();
			directories.Sort ();
			List<string > entries = new List<string> (directories);
			
			List<string > files = Directory.GetFiles (path).ToList ();
			files.Sort ();
			entries.AddRange (files);
			
			return entries.ToArray ();
		}
	}
}

