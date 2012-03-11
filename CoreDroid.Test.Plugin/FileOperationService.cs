using System;
using System.Runtime.Serialization;
using CoreDroid.Contract;
using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	[ServiceContract]
	public class CopyFileOperationService : OperationService<CopyFileOperationServiceThread, CopyFileOperationInfo>
	{
		[ServiceMember]
		public int Start (FileSystemItemInfo sourceItem, DirectoryItemInfo targetItem, bool quiet)
		{
			FileOperationServiceThread thread = this.Create (new CopyFileOperationInfo (sourceItem, targetItem, quiet));
			thread.Start ();
			return thread.Info.ID;
		}
		
		[ServiceMember]
		public void ResolveConflict (int id, bool overwrite)
		{
			if (!overwrite)
				this.Get (id).BlockedItems.Add (((CopyFileOperationInfo)this.Get (id).Info).Actual.Path);
			((CopyFileOperationInfo)this.Get (id).Info).RemoveConflict ();
			this.Get (id).InConflict = false;
		}
	}
		
	[ServiceContract]
	public class MoveFileOperationService : OperationService<CopyFileOperationServiceThread, MoveFileOperationInfo>
	{
		[ServiceMember]
		public int Start (FileSystemItemInfo sourceItem, DirectoryItemInfo targetItem, bool quiet)
		{
			FileOperationServiceThread thread = this.Create (new MoveFileOperationInfo (sourceItem, targetItem, quiet));
			thread.Start ();
			return thread.Info.ID;
		}
		
		[ServiceMember]
		public void ResolveConflict (int id, bool overwrite)
		{
			if (!overwrite)
				this.Get (id).BlockedItems.Add (((MoveFileOperationInfo)this.Get (id).Info).Actual.Path);
			((MoveFileOperationInfo)this.Get (id).Info).RemoveConflict ();
			this.Get (id).InConflict = false;
		}
	}
		
	[ServiceContract]
	public class DeleteFileOperationService : OperationService<DeleteFileOperationServiceThread, DeleteFileOperationInfo>
	{		
		[ServiceMember]
		public int Start (FileSystemItemInfo item)
		{
			FileOperationServiceThread thread = this.Create (new DeleteFileOperationInfo (item));
			thread.Start ();
			return thread.Info.ID;
		}
	}
		
	[ServiceContract]
	public class ChangeOwnerFileOperationService : OperationService<ChangeOwnerFileOperationServiceThread, ChangeOwnerFileOperationInfo>
	{		
		[ServiceMember]
		public int Start (FileSystemItemInfo item, ushort newUID, ushort newGID, bool recursive)
		{
			FileOperationServiceThread thread = this.Create (new ChangeOwnerFileOperationInfo (item, newUID, newGID, recursive));
			thread.Start ();
			return thread.Info.ID;
		}
	}
		
	[ServiceContract]
	public class ChangeModeFileOperationService : OperationService<ChangeModeFileOperationServiceThread, ChangeModeFileOperationInfo>
	{		
		[ServiceMember]
		public int Start (FileSystemItemInfo item, ushort newMode, bool recursive)
		{
			FileOperationServiceThread thread = this.Create (new ChangeModeFileOperationInfo (item, newMode, recursive));
			thread.Start ();
			return thread.Info.ID;
		}
	}
}

