using System;
using System.Runtime.Serialization;
using CoreDroid.Contract;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public abstract class FileOperationInfo : OperationInfo
	{
		[DataMember(Order = 0)]
		public FileSystemItemInfo Item { get; private set; }
		
		[DataMember(Order = 1)]
		public FileSystemItemInfo Actual { get; set; }
		
		public FileOperationInfo (FileSystemItemInfo item) : base()
		{
			this.Item = item;
		}
	}
	
	[DataContract]
	public class CopyFileOperationInfo:FileOperationInfo
	{
		[DataMember(Order = 0)]
		public DirectoryItemInfo TargetItem { get; private set; }
		
		[DataMember(Order = 1)]
		public ProgressInfo ActualProgress { get; set; }
		
		[DataMember(Order = 2)]
		public bool Quiet { get; private set; }
		
		[DataMember(Order = 3)]
		public FileSystemItemInfo ConflictItem { get; set; }
		
		[DataMember(Order = 4)]
		public bool ConflictOverwriteable { get; set; }
		
		public CopyFileOperationInfo (FileSystemItemInfo item, DirectoryItemInfo targetItem, bool quiet):base(item)
		{
			this.TargetItem = targetItem;
			this.Quiet = quiet;
		}
		
		public void RemoveConflict ()
		{
			this.ConflictItem = null;
			this.ConflictOverwriteable = false;
		}
	}
	
	public class MoveFileOperationInfo:CopyFileOperationInfo
	{		
		public MoveFileOperationInfo (FileSystemItemInfo item, DirectoryItemInfo targetItem, bool quiet) : base(item, targetItem, quiet)
		{
		}
	}
	
	public class DeleteFileOperationInfo:FileOperationInfo
	{
		public DeleteFileOperationInfo (FileSystemItemInfo item):base(item)
		{
		}
	}
	
	public abstract class ChangeFileOperationInfo:FileOperationInfo
	{		
		[DataMember(Order = 0)]
		public bool Recursive { get; private set; }
		
		public ChangeFileOperationInfo (FileSystemItemInfo item, bool recursive): base(item)
		{
			this.Recursive = recursive;
		}
	}
	
	public class ChangeOwnerFileOperationInfo:ChangeFileOperationInfo
	{
		[DataMember(Order = 0)]
		public uint NewUID { get; private set; }
		
		[DataMember(Order = 1)]
		public uint NewGID { get; private set; }
		
		public ChangeOwnerFileOperationInfo (FileSystemItemInfo item, uint newUID, uint newGID, bool recursive):base(item, recursive)
		{
			this.NewUID = newUID;
			this.NewGID = newGID;
		}
	}
	
	public class ChangeModeFileOperationInfo:ChangeFileOperationInfo
	{
		[DataMember(Order = 0)]
		public FilePermission? UserMode { get; private set; }

		[DataMember(Order = 1)]
		public FilePermission? GroupMode { get; private set; }

		[DataMember(Order = 2)]
		public FilePermission? OtherMode { get; private set; }

		[DataMember(Order = 3)]
		public bool? UIDBit { get; private set; }

		[DataMember(Order = 4)]
		public bool? GIDBit { get; private set; }

		[DataMember(Order = 5)]
		public bool? StickyBit { get; private set; }
		
		public ChangeModeFileOperationInfo (FileSystemItemInfo item, FilePermission? userMode, FilePermission? groupMode, FilePermission? otherMode, bool? uidBit, bool? gidBit, bool? stickyBit, bool recursive):base(item, recursive)
		{
			this.UserMode = userMode;
			this.GroupMode = groupMode;
			this.OtherMode = otherMode;
			this.UIDBit = uidBit;
			this.GIDBit = gidBit;
			this.StickyBit = stickyBit;
		}
	}
}

