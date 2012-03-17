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
		public uint CurrentUID { get; set; }
		
		[DataMember(Order = 1)]
		public IEnumerable<uint> CurrentGID { get; set; }
		
		[DataMember(Order = 2)]
		public DateTime LoadTime { get; private set; }
		
		[DataMember(Order = 3)]
		public string Path { get; private set; }
		
		[DataMember(Order = 4)]
		public string DirectoryPath { get; private set; }
		
		[DataMember(Order = 5)]
		public string Name { get; private set; }
		
		[DataMember(Order = 6)]
		public DateTime LastAccessTime { get; set; }

		[DataMember(Order = 7)]
		public DateTime LastModificationTime { get; set; }
		
		[DataMember(Order = 8)]
		public DateTime LastStatusChangeTime { get; set; }
		
		[DataMember(Order = 9)]
		public string LinkTarget { get; set; }
		
		[DataMember(Order = 10)]
		public string User { get; set; }
		
		[DataMember(Order = 11)]
		public uint UID { get; set; }
		
		[DataMember(Order = 12)]
		public string Group { get; set; }
		
		[DataMember(Order = 13)]
		public uint GID{ get; set; }
		
		[DataMember(Order = 14)]
		public FilePermission UserMode { get; set; }
		
		[DataMember(Order = 15)]
		public FilePermission GroupMode { get; set; }
		
		[DataMember(Order = 16)]
		public FilePermission OtherMode { get; set; }
		
		[DataMember(Order = 17)]
		public bool UIDBit { get; set; }
		
		[DataMember(Order = 18)]
		public bool GIDBit { get; set; }
		
		[DataMember(Order = 19)]
		public bool StickyBit { get; set; }
		
		public FileSystemItemInfo (string path)
		{
			this.LoadTime = DateTime.UtcNow;
			this.Path = path;
			this.Name = System.IO.Path.GetFileName (path);
			this.DirectoryPath = System.IO.Path.GetDirectoryName (path);
		}
		
		protected bool HasPermission (FilePermission mode)
		{
			return this.CurrentUID == 0 ||
				((this.OtherMode & mode) == mode) ||
				((this.GroupMode & mode) == mode && this.CurrentGID.Contains (this.GID)) ||
				((this.UserMode & mode) == mode && this.CurrentUID == this.UID);	
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
		public string MountDevice { get; set; }
		
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
		public long Size { get; set; }
		
		[DataMember(Order = 2)]
		public bool IsBlockDevice { get; set; }
		
		[DataMember(Order = 3)]
		public bool IsCharacterDevice { get; set; }
		
		[DataMember(Order = 4)]
		public bool IsSocket{ get; set; }
		
		[DataMember(Order = 5)]
		public bool IsFIFO{ get; set; }

		public FileItemInfo (string path) : base(path)
		{
			this.Extension = System.IO.Path.GetExtension (path);
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

