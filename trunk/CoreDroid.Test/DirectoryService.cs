using System;
using CoreDroid;

using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	public class DirectoryService : ServiceProxy
	{
		public const int DefaultLifeTime = 60;
		
		public DirectoryService ():base()
		{
		}
		
		public FileSystemItemInfo Get (string path)
		{
			return this.Get (path, DefaultLifeTime);
		}
		
		public FileSystemItemInfo Get (string path, int lifeTime)
		{
			return this.Call ("Get", typeof(FileSystemItemInfo), path, lifeTime) as FileSystemItemInfo;
		}
		
		public FileSystemItemInfo[] GetContents (DirectoryItemInfo directory)
		{
			return this.GetContents (directory, DefaultLifeTime);
		}
		
		public FileSystemItemInfo[] GetContents (DirectoryItemInfo directory, int lifeTime)
		{
			return this.Call ("GetContents", typeof(FileSystemItemInfo[]), directory, lifeTime) as FileSystemItemInfo[];
		}
	}
}

