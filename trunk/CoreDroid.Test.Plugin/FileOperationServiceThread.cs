using System;
using CoreDroid.Contract;

using DiskDroid.FileSystem.Contract;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace DiskDroid.FileSystem
{
	public abstract class FileOperationServiceThread : OperationServiceThread
	{
		public List<string> BlockedItems { get; private set; }
		
		protected DirectoryService directoryService = new DirectoryService ();

		protected sealed override ThreadStart Worker { get { return new ThreadStart (this.FileOperationWorker); } }
		
		protected abstract bool ShouldRecruse { get; }
		
		public FileOperationServiceThread (FileOperationInfo info) : base(info)
		{
			this.BlockedItems = new List<string> ();
		}
		
		private void FileOperationWorker ()
		{
			try {
				this.FileOperationItemWorker (((FileOperationInfo)this.Info).Item, true);
				this.CheckWorker ();
				this.PreWorker ();
				this.FileOperationItemWorker (((FileOperationInfo)this.Info).Item, false);
				this.PostWorker ();
				this.Info.IsFinished = true;
			} catch (Exception ex) {
				this.Info.Exception = new ExceptionInfo (ex);
			}
		}
		
		private void FileOperationItemWorker (FileSystemItemInfo item, bool check)
		{
			if (!this.BlockedItems.Contains (item.Path)) {
				((FileOperationInfo)this.Info).Actual = item;
			
				if (check) {
					this.CheckAction (item);
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
		}
		
		protected virtual void CheckAction (FileSystemItemInfo item)
		{
		}
		
		protected virtual void PreAction (FileSystemItemInfo item)
		{
		}
		
		protected virtual void PostAction (FileSystemItemInfo item)
		{
		}

		protected abstract void Action (FileSystemItemInfo item);
		
		protected virtual void CheckWorker ()
		{
		}
		
		protected virtual void PreWorker ()
		{
		}
		
		protected virtual void PostWorker ()
		{
		}
	}
	
	public class CopyFileOperationServiceThread:FileOperationServiceThread
	{
		public bool InConflict = false;
		
		protected override bool ShouldRecruse {
			get {
				return true;
			}
		}
		
		public CopyFileOperationServiceThread (CopyFileOperationInfo info):base(info)
		{
		}
		
		protected override void CheckWorker ()
		{
			base.CheckWorker ();
			if (!((CopyFileOperationInfo)this.Info).TargetItem.CanWrite ())
				throw new AccessViolationException ("you have no permission to write <" + ((CopyFileOperationInfo)this.Info).TargetItem.Path + ">");
		}
		
		private string GetTargetPath (FileSystemItemInfo item)
		{
			CopyFileOperationInfo info = (CopyFileOperationInfo)this.Info;
			string relativeTargetPath = item.DirectoryPath.Remove (0, info.Item.DirectoryPath.Length);
			if (relativeTargetPath.StartsWith ("/"))
				relativeTargetPath = relativeTargetPath.Remove (0, 1);
			return Path.Combine (info.TargetItem.Path, relativeTargetPath);
		}
		
		protected override void CheckAction (FileSystemItemInfo item)
		{
			base.CheckAction (item);
			if (!item.CanCopy ())
				throw new AccessViolationException ("you have no permission to copy <" + item.Path + ">");
			
			FileSystemItemInfo targetItem = this.directoryService.Get (Path.Combine (this.GetTargetPath (item), item.Name), 0);
			if (targetItem is FileItemInfo && !((CopyFileOperationInfo)this.Info).Quiet) {
				((CopyFileOperationInfo)this.Info).ConflictItem = targetItem;
				if (targetItem.CanDelete ()) {
					((CopyFileOperationInfo)this.Info).ConflictOverwriteable = true;
				} else {
					((CopyFileOperationInfo)this.Info).ConflictOverwriteable = false;
				}
				
				this.InConflict = true;
			}
			
			while (this.InConflict)
				Thread.Sleep (10);
			if (item is FileItemInfo)
				((CopyFileOperationInfo)this.Info).OverallProgress.Max += ((FileItemInfo)item).Size;
		}
		
		protected override void PreAction (FileSystemItemInfo item)
		{
			base.PreAction (item);
			
			if (item is DirectoryItemInfo && string.IsNullOrEmpty (item.LinkTarget)) {
				string targetPath = this.GetTargetPath (item);
				
				FileSystemItemInfo targetItem = this.directoryService.Get (Path.Combine (targetPath, item.Name), 0);
				
				if (targetItem is FileItemInfo)
					directoryService.Delete (targetItem);
				
				if (targetItem == null || targetItem is FileItemInfo)
					Directory.CreateDirectory (Path.Combine (targetPath, item.Name));
			}
		}
		
		protected override void Action (FileSystemItemInfo item)
		{
			CopyFileOperationInfo info = (CopyFileOperationInfo)this.Info;
			
			info.Actual = item;
			if (item is FileItemInfo) {
				info.ActualProgress = new ProgressInfo ();
				info.ActualProgress.Max = ((FileItemInfo)item).Size;
			}
			
			string targetPath = this.GetTargetPath (item);
			
			
			if (item is FileItemInfo || (item is DirectoryItemInfo && !string.IsNullOrEmpty (item.LinkTarget))) {
				FileSystemItemInfo targetItem = this.directoryService.Get (Path.Combine (this.GetTargetPath (item), item.Name), 0);
					
				if (targetItem != null)
					this.directoryService.Delete (targetItem);
				
				FileProgressWatcher watcher = new FileProgressWatcher (Path.Combine (targetPath, item.Name), info.ActualProgress);
				try {
					if (info is MoveFileOperationInfo)
						this.directoryService.Move (item, ((DirectoryItemInfo)this.directoryService.Get (targetPath, 0)));
					else {
						this.directoryService.Copy (item, ((DirectoryItemInfo)this.directoryService.Get (targetPath, 0)));
						FileSystemItemInfo newItem = this.directoryService.Get (System.IO.Path.Combine (targetPath, item.Name), 0);
						this.directoryService.ChangePermissions (newItem, item.UserMode, item.GroupMode, item.OtherMode, item.UIDBit, item.GIDBit, item.StickyBit);
						this.directoryService.ChangeOwner (newItem, item.UID, item.GID);
					}
				} finally {
					watcher.Stop ();
				}
			} else if (info is MoveFileOperationInfo && Directory.GetDirectories (item.Path).Length == 0 && Directory.GetFiles (item.Path).Length == 0) {
				this.directoryService.Delete (item);
			}
			
			if (item is FileItemInfo) {
				info.ActualProgress.Current = ((FileItemInfo)item).Size;
				info.OverallProgress.Current += ((FileItemInfo)item).Size;
			}
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
			this.directoryService.Delete (item);
		}
		
		protected override void CheckAction (FileSystemItemInfo item)
		{
			base.CheckAction (item);
			if (!item.CanDelete ())
				throw new AccessViolationException ("you have no permission to delete <" + item.Path + ">");
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
			this.directoryService.ChangeOwner (item, info.NewUID, info.NewGID);
		}
		
		protected override void CheckAction (FileSystemItemInfo item)
		{
			base.CheckAction (item);
			if (!item.CanChange ())
				throw new AccessViolationException ("you have no permission to change <" + item.Path + ">");
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
			this.directoryService.ChangePermissions (item,
			                                        info.UserMode.HasValue ? info.UserMode.Value : item.UserMode,
			                                        info.GroupMode.HasValue ? info.GroupMode.Value : item.GroupMode,
			                                        info.OtherMode.HasValue ? info.OtherMode.Value : item.OtherMode,
			                                        info.UIDBit.HasValue ? info.UIDBit.Value : item.UIDBit,
			                                        info.GIDBit.HasValue ? info.GIDBit.Value : item.GIDBit,
			                                        info.StickyBit.HasValue ? info.StickyBit.Value : item.StickyBit);
		}
		
		protected override void CheckAction (FileSystemItemInfo item)
		{
			base.CheckAction (item);
			if (!item.CanChange ())
				throw new AccessViolationException ("you have no permission to change <" + item.Path + ">");
		}
	}
}

