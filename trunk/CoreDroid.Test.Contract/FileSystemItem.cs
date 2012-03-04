using System;
using System.IO;

namespace DiskDroid.FileSystem
{
	public class FileSystemItem
	{
		public string Path { get; private set; }
		
		public string User { get; private set; }

		public ushort UID { get; private set; }

		public FileMode UserMode { get; private set; }
		
		public string Group { get; private set; }

		public ushort GID{ get; private set; }
		
		public FileMode GroupMode { get; private set; }
		
		public FileMode OthersMode { get; private set; }
		
		public FileSystemItem (string path)
		{
			this.Path = path;
		}
	}
	
	public class DirectoryItem : FileSystemItem
	{
		public DirectoryItem (string path):base(path)
		{
		}
	}
	
	public class FileItem : FileSystemItem
	{
		public FileItem (string path) : base(path)
		{
		}
	}
	
	public class SymbolicLinkItem : FileSystemItem
	{
		public SymbolicLinkItem (string path) : base(path)
		{
		}
	}

	public class MountItem:FileSystemItem
	{
		public string Target { get; private set; }
		
		public MountItem (string target, string path): base(path)
		{
		}
	}
	
	[Flags]
	public enum FileMode
	{
		Execute = 0x01,
		Write = 0x02,
		Read = 0x04
	}
}

