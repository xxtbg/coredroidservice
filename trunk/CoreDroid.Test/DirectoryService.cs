using System;
using CoreDroid;

using DiskDroid.FileSystem.Contract;
using System.Collections.Generic;

namespace DiskDroid.FileSystem
{
	public class DirectoryService : ServiceProxy
	{
		public const int DefaultLifeTime = 60;
		
		public FileSystemItemInfo Get (string path)
		{
			return this.Get (path, DefaultLifeTime);
		}
		
		public FileSystemItemInfo Get (string path, int lifeTime)
		{
			return this.Call ("Get", path, lifeTime) as FileSystemItemInfo;
		}
		
		public IEnumerable<FileSystemItemInfo> GetContents (DirectoryItemInfo directory)
		{
			return this.GetContents (directory, DefaultLifeTime);
		}
		
		public IEnumerable<FileSystemItemInfo> GetContents (DirectoryItemInfo directory, int lifeTime)
		{
			return this.Call ("GetContents", directory, lifeTime) as IEnumerable<FileSystemItemInfo>;
		}
	}
}

