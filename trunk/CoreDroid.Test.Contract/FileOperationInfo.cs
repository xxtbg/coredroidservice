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
		
		public CopyFileOperationInfo (FileSystemItemInfo item, DirectoryItemInfo targetItem):base(item)
		{
			this.TargetItem = targetItem;
		}
	}
	
	public class MoveFileOperationInfo:FileOperationInfo
	{
		[DataMember(Order = 0)]
		public DirectoryItemInfo TargetItem { get; private set; }
		
		[DataMember(Order = 1)]
		public ProgressInfo ActualProgress { get; set; }
		
		public MoveFileOperationInfo (FileSystemItemInfo item, DirectoryItemInfo targetItem) : base(item)
		{
			this.TargetItem = targetItem;
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
		public ushort NewUID { get; private set; }
		
		[DataMember(Order = 1)]
		public ushort NewGID { get; private set; }
		
		public ChangeOwnerFileOperationInfo (FileSystemItemInfo item, ushort newUID, ushort newGID, bool recursive):base(item, recursive)
		{
			this.NewUID = newUID;
			this.NewGID = newGID;
		}
	}
	
	public class ChangeModeFileOperationInfo:ChangeFileOperationInfo
	{
		[DataMember(Order = 0)]
		public ushort NewMode { get; private set; }
		
		public ChangeModeFileOperationInfo (FileSystemItemInfo item, ushort newMode, bool recursive):base(item, recursive)
		{
			this.NewMode = newMode;
		}
	}
}

