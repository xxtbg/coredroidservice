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
		public int Start (FileSystemItemInfo sourceItem, DirectoryItemInfo targetItem)
		{
			FileOperationServiceThread thread = this.Create (new CopyFileOperationInfo (sourceItem, targetItem));
			
			return thread.Info.ID;
		}
	}
		
	[ServiceContract]
	public class MoveFileOperationService : OperationService<MoveFileOperationServiceThread, MoveFileOperationInfo>
	{
		[ServiceMember]
		public int Start (FileSystemItemInfo sourceItem, DirectoryItemInfo targetItem)
		{
			FileOperationServiceThread thread = this.Create (new MoveFileOperationInfo (sourceItem, targetItem));
			
			return thread.Info.ID;
		}
	}
		
	[ServiceContract]
	public class DeleteFileOperationService : OperationService<DeleteFileOperationServiceThread, DeleteFileOperationInfo>
	{		
		[ServiceMember]
		public int Start (FileSystemItemInfo item)
		{
			FileOperationServiceThread thread = this.Create (new DeleteFileOperationInfo (item));
			
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
			
			return thread.Info.ID;
		}
	}
}

