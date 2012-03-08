using System;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public class FileSystemItemInfo
	{
		public static FileSystemItemInfo Get (string path)
		{
			Console.WriteLine ("path: " + path);
			if ((System.IO.File.GetAttributes (path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory) {
				return new DirectoryItemInfo (path);
			} else {
				return new FileItemInfo (path);
			}
		}
		
		[DataMember(Order = 0)]
		public DateTime LoadTime { get; private set; }
		
		[DataMember(Order = 1)]
		public string Path { get; private set; }
		
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
		public string LinkTarget { get; private set; }
		
		[DataMember(Order = 8)]
		public string User { get; private set; }
		
		[DataMember(Order = 9)]
		public ushort UID { get; private set; }
		
		[DataMember(Order = 10)]
		public FileMode UserMode { get; private set; }
		
		[DataMember(Order = 11)]
		public string Group { get; private set; }
		
		[DataMember(Order = 12)]
		public ushort GID{ get; private set; }
		
		[DataMember(Order = 13)]
		public FileMode GroupMode { get; private set; }
		
		[DataMember(Order = 14)]
		public FileMode OthersMode { get; private set; }
		
		public FileSystemItemInfo (string path)
		{
			this.LoadTime = DateTime.UtcNow;
			
			System.IO.FileInfo info = new System.IO.FileInfo (path);
			
			this.OnParseInfo (info);
			this.OnParseStat (this.Stat ());
		}
		
		protected virtual void OnParseInfo (System.IO.FileInfo info)
		{
			this.Path = info.FullName;
			this.DirectoryPath = info.DirectoryName;
			this.Name = info.Name;
			this.Size = info.Length;
			this.AccessTime = info.LastAccessTime;
			this.ModifyTime = info.LastWriteTime;
		}
		
		protected virtual void OnParseStat (string statOutput)
		{
			
		}
		
		private string Stat ()
		{			
			Process statProc = new Process ();
			statProc.EnableRaisingEvents = false;
			statProc.StartInfo.FileName = System.IO.Path.Combine (System.IO.Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "stat");
			statProc.StartInfo.Arguments = this.Path;
			statProc.StartInfo.UseShellExecute = false;
			statProc.StartInfo.RedirectStandardError = true;
			statProc.StartInfo.RedirectStandardOutput = true;
			statProc.Start ();
			statProc.WaitForExit ();
			
			string error = statProc.StandardError.ReadToEnd ();
			
			return statProc.StandardOutput.ReadToEnd ();
		}
	}
	
	[DataContract]
	public class DirectoryItemInfo : FileSystemItemInfo
	{
		[DataMember(Order = 0)]
		public string MountDevice { get; private set; }
		
		public DirectoryItemInfo (string path):base(path)
		{
		}
	}
	
	[DataContract]
	public class FileItemInfo : FileSystemItemInfo
	{
		[DataMember(Order = 0)]
		public string Extension { get; private set; }
		
		[DataMember(Order = 1)]
		public bool IsBlockDevice { get; private set; }
		
		[DataMember(Order = 2)]
		public bool IsCharacterDevice { get; private set; }

		public FileItemInfo (string path) : base(path)
		{
		}
		
		protected override void OnParseInfo (System.IO.FileInfo info)
		{
			base.OnParseInfo (info);
			
			this.Extension = info.Extension;			
		}
		
		protected override void OnParseStat (string statOutput)
		{
			base.OnParseStat (statOutput);
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

