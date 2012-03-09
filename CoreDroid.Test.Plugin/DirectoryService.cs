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
		private Dictionary<string, FileSystemItemInfo> cache = new Dictionary<string, FileSystemItemInfo> ();
		
		[ServiceMember]
		public FileSystemItemInfo Get (string path, int lifeTime)
		{
			if (path.Length > 1 && path.EndsWith ("/"))
				path = path.Remove (path.Length - 1);
			
			if (this.cache.ContainsKey (path) && DateTime.UtcNow > this.cache [path].LoadTime.AddSeconds (lifeTime))
				this.cache.Remove (path);
			
			if (!this.cache.ContainsKey (path)) {
				this.cache.Add (path, FileSystemItemInfo.Get (path));
			}
			
			return this.cache [path];
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

