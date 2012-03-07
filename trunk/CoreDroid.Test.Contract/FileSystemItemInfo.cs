using System;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public class FileSystemItemInfo
	{
		[DataMember(Order = 0)]
		public DateTime LoadTime { get; private set; }
		
		[DataMember(Order = 1)]
		public string ItemPath { get; private set; }
		
		[DataMember(Order = 2)]
		public string DirectoryPath { get ; private set; }
		
		[DataMember(Order = 3)]
		public string Name { get; private set; }
		
		[DataMember(Order = 4)]
		public long Size { get; private set; }
		
		[DataMember(Order = 5)]
		public DateTime AccessTime { get; private set; }

		[DataMember(Order = 6)]
		public DateTime ModifyTime { get; private set; }
		
		[DataMember(Order = 7)]
		public string User { get; private set; }
		
		[DataMember(Order = 8)]
		public ushort UID { get; private set; }
		
		[DataMember(Order = 9)]
		public FileMode UserMode { get; private set; }
		
		[DataMember(Order = 10)]
		public string Group { get; private set; }
		
		[DataMember(Order = 11)]
		public ushort GID{ get; private set; }
		
		[DataMember(Order = 12)]
		public FileMode GroupMode { get; private set; }
		
		[DataMember(Order = 13)]
		public FileMode OthersMode { get; private set; }
		
		public FileSystemItemInfo (string path)
		{
			this.LoadTime = DateTime.UtcNow;
			
			FileInfo info = new FileInfo (path);
			
			this.ItemPath = info.FullName;
			this.DirectoryPath = info.DirectoryName;
			this.Name = info.Name;
			this.Size = info.Length;
			this.AccessTime = info.LastAccessTime;
			this.ModifyTime = info.LastWriteTime;
		}
		
		private void Stat ()
		{			
			Process statProc = new Process ();
			statProc.EnableRaisingEvents = false;
			statProc.StartInfo.FileName = Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "stat");
			statProc.StartInfo.Arguments = this.ItemPath;
			statProc.StartInfo.UseShellExecute = false;
			statProc.StartInfo.RedirectStandardError = true;
			statProc.StartInfo.RedirectStandardOutput = true;
			statProc.Start ();
			statProc.WaitForExit ();
			
			string output = statProc.StandardOutput.ReadToEnd ();
			string error = statProc.StandardError.ReadToEnd ();			
		}
	}
	
	[DataContract]
	public class DirectoryItemInfo : FileSystemItemInfo
	{
		public DirectoryItemInfo (string path):base(path)
		{
		}
	}
	
	[DataContract]
	public class FileItemInfo : FileSystemItemInfo
	{
		[DataMember(Order = 0)]
		public string Extension { get; private set; }

		public FileItemInfo (string path) : base(path)
		{
			FileInfo info = new FileInfo (path);
			
			this.Extension = info.Extension;
		}
	}
	
	[DataContract]
	public class SymbolicLinkItemInfo : FileSystemItemInfo
	{
		[DataMember(Order = 0)]
		public string Target { get; private set; }
		
		public SymbolicLinkItemInfo (string path, string target) : base(path)
		{
			this.Target = target;
		}
	}
	
	[DataContract]
	public class MountItemInfo : DirectoryItemInfo
	{
		[DataMember(Order = 0)]
		public string Device { get; private set; }
		
		public MountItemInfo (string device, string path): base(path)
		{
			this.Device = device;
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

