using System;
using CoreDroid;

using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	public class CopyFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item, DirectoryItemInfo targetItem)
		{
			return (int)this.Call ("Start", item, targetItem);
		}
	}
	
	public class MoveFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item, DirectoryItemInfo targetItem)
		{
			return (int)this.Call ("Start", item, targetItem);
		}
	}
	
	public class DeleteFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item)
		{
			return (int)this.Call ("Start", item);
		}
	}
	
	public class ChangeOwnerFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item, ushort newUID, ushort newGID, bool recursive)
		{
			return (int)this.Call ("Start", item, newUID, newGID, recursive);
		}
	}
	
	public class ChangeModeFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item, ushort newMode, bool recursive)
		{
			return (int)this.Call ("Start", item, newMode, recursive);
		}
	}
}

