using System;
using CoreDroid;

using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	public class CopyFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item, DirectoryItemInfo targetItem, bool quiet)
		{
			return (int)this.Call ("Start", item, targetItem, quiet);
		}
		
		public void ResolveConflict (int id, bool overwrite)
		{
			this.Call ("ResolveConflict", id, overwrite);
		}
	}
	
	public class MoveFileOperationService : OperationServiceProxy
	{
		public int Start (FileSystemItemInfo item, DirectoryItemInfo targetItem, bool quiet)
		{
			return (int)this.Call ("Start", item, targetItem, quiet);
		}
		
		public void ResolveConflict (int id, bool overwrite)
		{
			this.Call ("ResolveConflict", id, overwrite);
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

