using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using CoreDroid.Extensions;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public abstract class FileSystemItemInfo
	{		
		[DataMember(Order = 0)]
		public DateTime LoadTime { get; private set; }
		
		[DataMember(Order = 1)]
		public string Path { get; private set; }
		
		[DataMember(Order = 2)]
		public string DirectoryPath { get ; private set; }
		
		[DataMember(Order = 3)]
		public string Name { get; private set; }
		
		[DataMember(Order = 4)]
		public DateTime AccessTime { get; private set; }

		[DataMember(Order = 5)]
		public DateTime ModifyTime { get; private set; }
		
		[DataMember(Order = 6)]
		public string LinkTarget { get; private set; }
		
		[DataMember(Order = 7)]
		public string User { get; private set; }
		
		[DataMember(Order = 8)]
		public ushort UID { get; private set; }
		
		[DataMember(Order = 9)]
		public string Group { get; private set; }
		
		[DataMember(Order = 10)]
		public ushort GID{ get; private set; }
		
		[DataMember(Order = 11)]
		public FilePermission UserMode { get; private set; }
		
		[DataMember(Order = 12)]
		public FilePermission GroupMode { get; private set; }
		
		[DataMember(Order = 13)]
		public FilePermission OthersMode { get; private set; }
		
		public FileSystemItemInfo (string path)
		{
			this.LoadTime = DateTime.UtcNow;
			this.Path = path;
			this.Name = System.IO.Path.GetFileName (path);
			this.DirectoryPath = System.IO.Path.GetDirectoryName (path);
		}
		
		protected bool HasPermission (FilePermission mode)
		{
			return CurrentUID == 0 ||
				((this.OthersMode & mode) == mode) ||
				((this.GroupMode & mode) == mode && CurrentGID.Contains (this.GID)) ||
				((this.UserMode & mode) == mode && CurrentUID == this.UID);	
		}
		
		public bool CanRead ()
		{
			return this.HasPermission (FilePermission.Read);
		}
		
		public bool CanWrite ()
		{
			return this.HasPermission (FilePermission.Write);
		}
		
		public bool CanExecute ()
		{
			return this.HasPermission (FilePermission.Execute);
		}
		
		public bool CanCopy ()
		{
			return this.CanRead ();
		}
		
		public bool CanMove ()
		{
			return this.CanRead () && this.CanWrite ();
		}
		
		public bool CanDelete ()
		{
			return this.CanWrite ();
		}
		
		public bool CanChange ()
		{
			return this.CanWrite ();
		}
		
		public void SetExpired ()
		{
			this.LoadTime = DateTime.MinValue;
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
		public long Size { get; private set; }
		
		[DataMember(Order = 2)]
		public bool IsBlockDevice { get; private set; }
		
		[DataMember(Order = 3)]
		public bool IsCharacterDevice { get; private set; }

		public FileItemInfo (string path) : base(path)
		{
		}
	}
	
	[Flags]
	public enum FilePermission : ushort
	{
		Execute = 0x01,
		Write = 0x02,
		Read = 0x04
	}
}

