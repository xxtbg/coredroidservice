using System;
using System.Linq;
using CoreDroid.Contract;
using System.Collections.Generic;
using System.IO;

using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	[ServiceContract]
	public class DirectoryService
	{		
		[ServiceMember]
		public FileSystemItemInfo Get (string path, int lifeTime)
		{			
			return FileSystemItemInfo.Get (path, lifeTime);
		}
		
		[ServiceMember]
		public IEnumerable<FileSystemItemInfo> GetContents (DirectoryItemInfo directory, int lifeTime)
		{
			List<FileSystemItemInfo> items = new List<FileSystemItemInfo> ();
			items.AddRange (Directory.GetDirectories (directory.Path).OrderBy (d => d).Select (d => this.Get (d, lifeTime)));
			items.AddRange (Directory.GetFiles (directory.Path).OrderBy (f => f).Select (f => this.Get (f, lifeTime)));
			
			return items.AsEnumerable ();
		}
	}
}

