using System;
using CoreDroid.Contract;

using DiskDroid.FileSystem.Contract;
using System.Threading;
using System.IO;

namespace DiskDroid.FileSystem
{
	public abstract class FileOperationServiceThread : OperationServiceThread
	{		
		private DirectoryService directoryService = new DirectoryService ();

		protected sealed override ThreadStart Worker { get { return new ThreadStart (this.FileOperationWorker); } }
		
		protected abstract bool ShouldRecruse { get; }
		
		public FileOperationServiceThread (FileOperationInfo info) : base(info)
		{
		}
		
		private void FileOperationWorker ()
		{
			try {
				this.FileOperationItemWorker (((FileOperationInfo)this.Info).Item, true);
				if (!this.CheckWorker ())
					new FileOperationCheckFailedException ();
				this.PreWorker ();
				this.FileOperationItemWorker (((FileOperationInfo)this.Info).Item, false);
				this.PostWorker ();
			} catch (Exception ex) {
				this.Info.Exception = new ExceptionInfo (ex);
			}
		}
		
		private void FileOperationItemWorker (FileSystemItemInfo item, bool check)
		{
			((FileOperationInfo)this.Info).Actual = item;
			
			if (check) {
				if (!this.CheckAction (item)) {
					throw(new FileOperationCheckFailedException ());
				}
			} else {
				this.PreAction (item);
			}
			
			if (this.ShouldRecruse && item is DirectoryItemInfo) {				
				foreach (FileSystemItemInfo contentItem in this.directoryService.GetContents ((DirectoryItemInfo)item, 0)) {
					this.FileOperationItemWorker (contentItem, check);
				}				
			}
			
			if (!check) {
				this.Action (item);
				this.PostAction (item);
			}
		}
		
		protected virtual bool CheckAction (FileSystemItemInfo item)
		{
			return true;
		}
		
		protected virtual void PreAction (FileSystemItemInfo item)
		{
		}
		
		protected virtual void PostAction (FileSystemItemInfo item)
		{
		}

		protected abstract void Action (FileSystemItemInfo item);
		
		protected virtual bool CheckWorker ()
		{
			return true;
		}
		
		protected virtual void PreWorker ()
		{
		}
		
		protected virtual void PostWorker ()
		{
		}
	}
	
	public class FileOperationCheckFailedException:Exception
	{
		public FileOperationCheckFailedException ():base()
		{
		}
	}
	
	public class CopyFileOperationServiceThread:FileOperationServiceThread
	{
		protected override bool ShouldRecruse {
			get {
				return true;
			}
		}
		
		public CopyFileOperationServiceThread (CopyFileOperationInfo info):base(info)
		{
		}
		
		protected override bool CheckWorker ()
		{
			return base.CheckWorker () && ((CopyFileOperationInfo)this.Info).TargetItem.CanWrite ();
		}
		
		protected override bool CheckAction (FileSystemItemInfo item)
		{
			return base.CheckAction (item) && item.CanCopy ();
		}
		
		protected override void PreAction (FileSystemItemInfo item)
		{
			base.PreAction (item);
			
			if (item is DirectoryItemInfo && string.IsNullOrEmpty (item.LinkTarget)) {
				CopyFileOperationInfo info = (CopyFileOperationInfo)this.Info;
				string targetPath = Path.Combine (info.TargetItem.Path, item.Path.Remove (0, info.Item.DirectoryPath.Length));
				
				Directory.CreateDirectory (targetPath);
			}
		}
		
		protected override void Action (FileSystemItemInfo item)
		{
			CopyFileOperationInfo info = (CopyFileOperationInfo)this.Info;
			
			info.Actual = item;
			info.ActualProgress = new ProgressInfo (item.Size);
			
			string targetPath = Path.Combine (info.TargetItem.Path, item.Path.Remove (0, info.Item.DirectoryPath.Length));
			if (item is FileItemInfo || (item is DirectoryItemInfo && !string.IsNullOrEmpty (item.LinkTarget)))
				item.Copy ((DirectoryItemInfo)FileSystemItemInfo.Get (targetPath));
			
			info.ActualProgress.Current = item.Size;
		}
	}
	
	public class MoveFileOperationServiceThread:FileOperationServiceThread
	{
		protected override bool ShouldRecruse {
			get {
				return true;
			}
		}
		
		public MoveFileOperationServiceThread (MoveFileOperationInfo info):base(info)
		{
		}
		
		protected override bool CheckWorker ()
		{
			return base.CheckWorker () && ((CopyFileOperationInfo)this.Info).TargetItem.CanWrite ();
		}
		
		protected override bool CheckAction (FileSystemItemInfo item)
		{
			return base.CheckAction (item) && item.CanMove ();
		}
		
		protected override void PreAction (FileSystemItemInfo item)
		{
			base.PreAction (item);
			
			if (item is DirectoryItemInfo && string.IsNullOrEmpty (item.LinkTarget)) {
				MoveFileOperationInfo info = (MoveFileOperationInfo)this.Info;
				string targetPath = Path.Combine (info.TargetItem.Path, item.Path.Remove (0, info.Item.DirectoryPath.Length));
				
				Directory.CreateDirectory (targetPath);
			}
		}
		
		protected override void Action (FileSystemItemInfo item)
		{
			MoveFileOperationInfo info = (MoveFileOperationInfo)this.Info;
			
			info.Actual = item;
			info.ActualProgress = new ProgressInfo (item.Size);
			
			string targetPath = Path.Combine (info.TargetItem.Path, item.Path.Remove (0, info.Item.DirectoryPath.Length));
			if (item is FileItemInfo || (item is DirectoryItemInfo && !string.IsNullOrEmpty (item.LinkTarget)))
				item.Move ((DirectoryItemInfo)FileSystemItemInfo.Get (targetPath));
			else
				item.Delete ();
			
			info.ActualProgress.Current = item.Size;
		}
	}
	
	public class DeleteFileOperationServiceThread:FileOperationServiceThread
	{
		protected override bool ShouldRecruse {
			get {
				return true;
			}
		}
		
		public DeleteFileOperationServiceThread (DeleteFileOperationInfo info):base(info)
		{
		}
		
		protected override void Action (FileSystemItemInfo item)
		{
			item.Delete ();
		}
		
		protected override bool CheckAction (FileSystemItemInfo item)
		{
			return base.CheckAction (item) && item.CanDelete ();
		}
	}
	
	public class ChangeOwnerFileOperationServiceThread:FileOperationServiceThread
	{
		protected override bool ShouldRecruse {
			get {
				return ((ChangeFileOperationInfo)this.Info).Recursive;
			}
		}
		
		public ChangeOwnerFileOperationServiceThread (ChangeOwnerFileOperationInfo info):base(info)
		{
		}
		
		protected override void Action (FileSystemItemInfo item)
		{
			ChangeOwnerFileOperationInfo info = (ChangeOwnerFileOperationInfo)this.Info;
			item.ChangeOwner (info.NewUID, info.NewGID);
		}
		
		protected override bool CheckAction (FileSystemItemInfo item)
		{
			return base.CheckAction (item) && item.CanChange ();
		}
	}
	
	public class ChangeModeFileOperationServiceThread:FileOperationServiceThread
	{
		protected override bool ShouldRecruse {
			get {
				return ((ChangeFileOperationInfo)this.Info).Recursive;
			}
		}
		
		public ChangeModeFileOperationServiceThread (ChangeModeFileOperationInfo info):base(info)
		{
		}
		
		protected override void Action (FileSystemItemInfo item)
		{
			ChangeModeFileOperationInfo info = (ChangeModeFileOperationInfo)this.Info;
			item.ChangeMode (info.NewMode);
		}
		
		protected override bool CheckAction (FileSystemItemInfo item)
		{
			return base.CheckAction (item) && item.CanChange ();
		}
	}
}

