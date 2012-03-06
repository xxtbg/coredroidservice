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
		[DataMember]
		public string ItemPath { get; private set; }
		
		[DataMember]
		public string DirectoryPath { get ; private set; }
		
		[DataMember]
		public string Name { get; private set; }
		
		[DataMember]
		public string User { get; private set; }
		
		[DataMember]
		public ushort UID { get; private set; }
		
		[DataMember]
		public FileMode UserMode { get; private set; }
		
		[DataMember]
		public string Group { get; private set; }
		
		[DataMember]
		public ushort GID{ get; private set; }
		
		[DataMember]
		public FileMode GroupMode { get; private set; }
		
		[DataMember]
		public FileMode OthersMode { get; private set; }
		
		public FileSystemItemInfo (string path)
		{
			this.ItemPath = path;
			
			this.DirectoryPath = Path.GetDirectoryName (this.ItemPath);
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
		[DataMember]
		public string Extension { get; private set; }

		public FileItemInfo (string path) : base(path)
		{
			this.Extension = Path.GetExtension (this.ItemPath);
		}
	}
	
	[DataContract]
	public class SymbolicLinkItemInfo : FileSystemItemInfo
	{
		[DataMember]
		public string Target { get; private set; }
		
		public SymbolicLinkItemInfo (string path, string target) : base(path)
		{
			this.Target = target;
		}
	}
	
	[DataContract]
	public class MountItemInfo : FileSystemItemInfo
	{
		[DataMember]
		public string Target { get; private set; }
		
		public MountItemInfo (string target, string path): base(path)
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

