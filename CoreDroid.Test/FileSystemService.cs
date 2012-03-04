using System;
using CoreDroid;

namespace DiskDroid.FileSystem
{
	public class FileSystemService : ServiceProxy
	{
		public FileSystemService ():base()
		{
		}
		
		public string[] List (string path)
		{
			return this.Call ("List", typeof(string[]), path) as string[];
		}
	}
}

