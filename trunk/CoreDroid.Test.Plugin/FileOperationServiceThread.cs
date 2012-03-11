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
		
		private DirectoryService directoryService = new DirectoryService ();

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
			
			FileSystemItemInfo targetItem = FileSystemItemInfo.GetOrNull (Path.Combine (this.GetTargetPath (item), item.Name));
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
				
				FileSystemItemInfo targetItem = FileSystemItemInfo.GetOrNull (Path.Combine (targetPath, item.Name));
				
				if (targetItem is FileItemInfo)
					targetItem.Delete ();
				
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
				FileSystemItemInfo targetItem = FileSystemItemInfo.GetOrNull (Path.Combine (this.GetTargetPath (item), item.Name));
					
				if (targetItem != null)
					targetItem.Delete ();
				
				FileProgressWatcher watcher = new FileProgressWatcher (Path.Combine (targetPath, item.Name), info.ActualProgress);
				try {
					if (info is MoveFileOperationInfo)
						item.Move ((DirectoryItemInfo)FileSystemItemInfo.Get (targetPath));
					else
						item.Copy ((DirectoryItemInfo)FileSystemItemInfo.Get (targetPath));
				} finally {
					watcher.Stop ();
				}
			} else if (info is MoveFileOperationInfo && Directory.GetDirectories (item.Path).Length == 0 && Directory.GetFiles (item.Path).Length == 0) {
				item.Delete ();
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
			item.Delete ();
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
			item.ChangeOwner (info.NewUID, info.NewGID);
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
			item.ChangeMode (info.NewMode);
		}
		
		protected override void CheckAction (FileSystemItemInfo item)
		{
			base.CheckAction (item);
			if (!item.CanChange ())
				throw new AccessViolationException ("you have no permission to change <" + item.Path + ">");
		}
	}
}

