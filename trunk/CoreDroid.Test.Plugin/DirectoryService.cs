using System;
using System.Linq;
using CoreDroid.Contract;
using System.Collections.Generic;
using System.IO;

using CoreDroid.Native.Lib.C.Sys;
using DiskDroid.FileSystem.Contract;
using CoreDroid.Native.Lib.C;

namespace DiskDroid.FileSystem
{
	[ServiceContract]
	public class DirectoryService
	{
		private Dictionary<string, FileSystemItemInfo> cache = new Dictionary<string, FileSystemItemInfo> ();
		private int? currentUID = null;
		
		protected int CurrentUID {
			get {
				if (currentUID == null) {
					currentUID = Convert.ToUInt16 ("id".Run ("-u"));
				}
				
				return currentUID.Value;
			}
		}
		
		private IEnumerable<int> currentGID = null;
		
		protected IEnumerable<int> CurrentGID {
			get {
				if (currentGID == null) {
					currentGID = "id".Run ("-G").Split (' ').Select (g => Convert.ToUInt16 (g));
				}
				
				return currentGID;
			}
		}
		
		[ServiceMember]
		public FileSystemItemInfo Get (string path, int lifeTime)
		{
			// fix path
			if (path.Length > 1 && path.EndsWith ("/"))
				path = path.Remove (path.Length - 1);
			
			// get filestat
			FileStat stat = StatH.Stat (path);
			
			if (this.cache.ContainsKey (path) && DateTime.UtcNow > this.cache [path].LoadTime.AddSeconds (lifeTime))
				this.cache.Remove (path);
			
			if (!this.cache.ContainsKey (path)) {
				this.cache.Add (path, InternalGet (path));
			}
			
			return cache [path];
		}
		
		[ServiceMember]
		public IEnumerable<FileSystemItemInfo> GetContents (DirectoryItemInfo directory, int lifeTime)
		{
			List<FileSystemItemInfo> items = new List<FileSystemItemInfo> ();
			items.AddRange (Directory.GetDirectories (directory.Path).OrderBy (d => d).Select (d => this.Get (d, lifeTime)));
			items.AddRange (Directory.GetFiles (directory.Path).OrderBy (f => f).Select (f => this.Get (f, lifeTime)));
			
			return items.AsEnumerable ();
		}
		
		public void Copy (FileSystemItemInfo source, DirectoryItemInfo target)
		{
			if (!source.CanCopy () && target.CanWrite ())
				throw(new AccessViolationException ("you are not allowed to copy <" + source.Path + ">"));
			
			if (!(source is FileItemInfo) && string.IsNullOrEmpty (source.LinkTarget))
				throw(new ArgumentException ("can only copy files and symbolic links, recursive operations are not supported by directory service"));
			
			if (string.IsNullOrEmpty (source.LinkTarget)) {
				File.Copy (source.Path, Path.Combine (target.Path, source.Name));
			} else {
				UnistdH.Symlink (source.LinkTarget, Path.Combine (target.Path, source.Name));
			}
		}
		
		public void Move (FileSystemItemInfo source, DirectoryItemInfo target)
		{
			if (!source.CanMove () && target.CanWrite ())
				throw(new AccessViolationException ("you are not allowed to move <" + source.Path + ">"));
			
			if (!(source is FileItemInfo) && string.IsNullOrEmpty (source.LinkTarget))
				throw(new ArgumentException ("can only copy files and symbolic links, recursive operations are not supported by directory service"));
			
			if (string.IsNullOrEmpty (source.LinkTarget)) {
				File.Move (source.Path, Path.Combine (target.Path, source.Name));
			} else {					
				UnistdH.Symlink (source.LinkTarget, Path.Combine (target.Path, source.Name));
				this.Delete (source);
			}
		}
		
		public void Delete (FileSystemItemInfo item)
		{
			if (!item.CanDelete ())
				throw(new AccessViolationException ("you are not allowed to move <" + item.Path + ">"));
			
			if (item is DirectoryItemInfo && string.IsNullOrEmpty (item.LinkTarget)) {
				Directory.Delete (item.Path);
			} else {
				UnistdH.Unlink (item.Path);
			}
			
			item.LoadTime = DateTime.MinValue;
		}
		
		public void ChangeOwner (FileSystemItemInfo item, int newUID, int newGID)
		{
			if (!this.CanChange ())
				throw(new AccessViolationException ("you are not allowed to change <" + this.Path + ">"));
			
			UnistdH.Chown (item.Path, newUID, newGID);
			
			item.SetExpired ();
		}
		
		public void ChangePermissions (FileSystemItemInfo item, FilePermission newUserPermissions, FilePermission newGroupPermission, FilePermission newOtherPermission)
		{
			if (!this.CanChange ())
				throw(new AccessViolationException ("you are not allowed to change <" + this.Path + ">"));
			
			FileStat stat = StatH.Stat (path);
			FileMode mode = stat.Mode;
			
			mode = mode ^ FileMode.PermissionOtherExecute ^ FileMode.PermissionOtherWrite ^ FileMode.PermissionOtherRead;
			mode = mode ^ FileMode.PermissionGroupExecute ^ FileMode.PermissionGroupWrite ^ FileMode.PermissionGroupRead;
			mode = mode ^ FileMode.PermissionUserExecute ^ FileMode.PermissionUserWrite ^ FileMode.PermissionUserRead;
			
			if ((newOtherPermission & FilePermission.Execute) == FilePermission.Execute)
				mode = mode | FileMode.PermissionOtherExecute;
			if ((newOtherPermission & FilePermission.Write) == FilePermission.Write)
				mode = mode | FileMode.PermissionOtherWrite;
			if ((newOtherPermission & FilePermission.Read) == FilePermission.Read)
				mode = mode | FileMode.PermissionOtherRead;
			
			if ((newGroupPermission & FilePermission.Execute) == FilePermission.Execute)
				mode = mode | FileMode.PermissionGroupExecute;
			if ((newGroupPermission & FilePermission.Write) == FilePermission.Write)
				mode = mode | FileMode.PermissionGroupWrite;
			if ((newGroupPermission & FilePermission.Read) == FilePermission.Read)
				mode = mode | FileMode.PermissionGroupRead;
			
			if ((newUserPermission & FilePermission.Execute) == FilePermission.Execute)
				mode = mode | FileMode.PermissionUserExecute;
			if ((newUserPermission & FilePermission.Write) == FilePermission.Write)
				mode = mode | FileMode.PermissionUserWrite;
			if ((newUserPermission & FilePermission.Read) == FilePermission.Read)
				mode = mode | FileMode.PermissionUserRead;			
			
			StatH.Chmod (item.Path, mode);
			
			item.SetExpired ();
		}
	}
}

