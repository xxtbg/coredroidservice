using System;
using System.Linq;
using CoreDroid.Contract;
using System.Collections.Generic;

using DiskDroid.FileSystem.Contract;
using Mono.Unix.Native;
using System.Text;

namespace DiskDroid.FileSystem
{
	[ServiceContract]
	public class DirectoryService
	{
		public static readonly DateTime UnixEpoch = new DateTime (1970, 1, 1);

		public static DateTime UnixToDateTime (long unix)
		{
			return UnixEpoch.Add (TimeSpan.FromSeconds (unix)).ToLocalTime ();
		}
		
		private Dictionary<string, FileSystemItemInfo> cache = new Dictionary<string, FileSystemItemInfo> ();
		private uint? currentUID = null;
		
		protected uint CurrentUID {
			get {
				if (currentUID == null) {
					currentUID = Syscall.getuid ();
				}
				
				return currentUID.Value;
			}
		}
		
		private IEnumerable<uint> currentGID = null;
		
		protected IEnumerable<uint> CurrentGID {
			get {
				if (currentGID == null) {
					List<uint> groupList = new List<uint> ();
					groupList.Add (Syscall.getgid ());
					
					uint[] buf = new uint[32];
					int amount = Syscall.getgroups (32, buf);
                        
					uint[] ret = new uint[amount];
					Array.Copy (buf, ret, amount);
					
					groupList.AddRange (ret);
					currentGID = groupList.ToArray ();
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
			
			if (this.cache.ContainsKey (path) && DateTime.UtcNow > this.cache [path].LoadTime.AddSeconds (lifeTime))
				this.cache.Remove (path);
			
			if (!this.cache.ContainsKey (path)) {
				Stat? stat = new Stat ();
				Stat tmpStat = new Stat ();
				stat = Syscall.stat (path, out tmpStat) == 0 ? tmpStat as Stat? : null;
				
				string symlinkPath = null;
				Stat? usingStat = new Stat ();
				if (stat.HasValue) {
					if ((stat.Value.st_mode & FilePermissions.S_IFLNK) == FilePermissions.S_IFLNK) {
						StringBuilder sb = new StringBuilder ();
						Syscall.readlink (path, sb);
						symlinkPath = sb.ToString ();
						tmpStat = new Stat ();
						usingStat = Syscall.stat (path, out tmpStat) == 0 ? tmpStat as Stat? : null;
					} else
						usingStat = stat;
				}
				
				if (usingStat != null) {
					this.cache.Add (path, this.CreateItem (path, usingStat.Value, symlinkPath));
				} else {
					return null;
				}
			}
			
			return cache [path];
		}
		
		private FileSystemItemInfo CreateItem (string path, Stat stat, string symlinkPath)
		{
			FileSystemItemInfo item = null;
				
			if ((stat.st_mode & FilePermissions.S_IFDIR) == FilePermissions.S_IFDIR) {
				item = new DirectoryItemInfo (path);
					
				// TODO: search for mount device
			} else {
				item = new FileItemInfo (path);
				((FileItemInfo)item).Size = stat.st_size;		// i know, also directories, links and whatever have a size, but I want to have the pure data size shown
				((FileItemInfo)item).IsBlockDevice = (stat.st_mode & FilePermissions.S_IFBLK) == FilePermissions.S_IFBLK;
				((FileItemInfo)item).IsCharacterDevice = (stat.st_mode & FilePermissions.S_IFCHR) == FilePermissions.S_IFCHR;
				((FileItemInfo)item).IsSocket = (stat.st_mode & FilePermissions.S_IFSOCK) == FilePermissions.S_IFSOCK;
				((FileItemInfo)item).IsFIFO = (stat.st_mode & FilePermissions.S_IFIFO) == FilePermissions.S_IFIFO;
			}
				
			item.LastAccessTime = UnixToDateTime (stat.st_atime);
			item.LastModificationTime = UnixToDateTime (stat.st_mtime);
			item.LastStatusChangeTime = UnixToDateTime (stat.st_ctime);
				
			item.LinkTarget = symlinkPath;
				
			item.UID = stat.st_uid;
			item.User = Mono.Posix.Syscall.getusername (Convert.ToInt32 (item.UID));
			item.GID = stat.st_gid;
			item.Group = Mono.Posix.Syscall.getgroupname (Convert.ToInt32 (item.GID));
				
			item.OtherMode = 0;
			item.GroupMode = 0;
			item.UserMode = 0;
				
			if ((stat.st_mode & FilePermissions.S_IXOTH) == FilePermissions.S_IXOTH)
				item.OtherMode = item.OtherMode | FilePermission.Execute;
			if ((stat.st_mode & FilePermissions.S_IWOTH) == FilePermissions.S_IWOTH)
				item.OtherMode = item.OtherMode | FilePermission.Write;
			if ((stat.st_mode & FilePermissions.S_IROTH) == FilePermissions.S_IROTH)
				item.OtherMode = item.OtherMode | FilePermission.Read;
				
			if ((stat.st_mode & FilePermissions.S_IXGRP) == FilePermissions.S_IXGRP)
				item.GroupMode = item.GroupMode | FilePermission.Execute;
			if ((stat.st_mode & FilePermissions.S_IWGRP) == FilePermissions.S_IWGRP)
				item.GroupMode = item.GroupMode | FilePermission.Write;
			if ((stat.st_mode & FilePermissions.S_IRGRP) == FilePermissions.S_IRGRP)
				item.GroupMode = item.GroupMode | FilePermission.Read;
				
			if ((stat.st_mode & FilePermissions.S_IXUSR) == FilePermissions.S_IXUSR)
				item.UserMode = item.UserMode | FilePermission.Execute;
			if ((stat.st_mode & FilePermissions.S_IWUSR) == FilePermissions.S_IWUSR)
				item.UserMode = item.UserMode | FilePermission.Write;
			if ((stat.st_mode & FilePermissions.S_IRUSR) == FilePermissions.S_IRUSR)
				item.UserMode = item.UserMode | FilePermission.Read;
			
			item.UIDBit = (stat.st_mode & FilePermissions.S_ISUID) == FilePermissions.S_ISUID;
			item.GIDBit = (stat.st_mode & FilePermissions.S_ISGID) == FilePermissions.S_ISGID;
			item.StickyBit = (stat.st_mode & FilePermissions.S_ISVTX) == FilePermissions.S_ISVTX;
			
			return item;
		}
		
		[ServiceMember]
		public IEnumerable<FileSystemItemInfo> GetContents (DirectoryItemInfo directory, int lifeTime)
		{
			directory = this.Reload<DirectoryItemInfo> (directory);
			if (!directory.CanRead () && directory.CanExecute ())
				throw(new AccessViolationException ("you are not allowed to read and execute <" + directory.Path + ">"));
			
			List<FileSystemItemInfo> items = new List<FileSystemItemInfo> ();
			items.AddRange (System.IO.Directory.GetDirectories (directory.Path).OrderBy (d => d).Select (d => this.Get (d, lifeTime)));
			items.AddRange (System.IO.Directory.GetFiles (directory.Path).OrderBy (f => f).Select (f => this.Get (f, lifeTime)));
			
			return items.AsEnumerable ();
		}
		
		public void Copy (FileSystemItemInfo source, DirectoryItemInfo target)
		{
			source = this.Reload<FileSystemItemInfo> (source);
			target = this.Reload<DirectoryItemInfo> (target);
			if (!source.CanCopy () && target.CanWrite ())
				throw(new AccessViolationException ("you are not allowed to copy <" + source.Path + ">"));
			
			if (!(source is FileItemInfo) && string.IsNullOrEmpty (source.LinkTarget))
				throw(new ArgumentException ("can only copy files and symbolic links, recursive operations are not supported by directory service"));
			
			if (string.IsNullOrEmpty (source.LinkTarget)) {
				System.IO.File.Copy (source.Path, System.IO.Path.Combine (target.Path, source.Name));
			} else {
				Syscall.symlink (source.LinkTarget, System.IO.Path.Combine (target.Path, source.Name));
			}
		}
		
		public void Move (FileSystemItemInfo source, DirectoryItemInfo target)
		{
			source = this.Reload<FileSystemItemInfo> (source);
			target = this.Reload<DirectoryItemInfo> (target);
			if (!source.CanMove () && target.CanWrite ())
				throw(new AccessViolationException ("you are not allowed to move <" + source.Path + ">"));
			
			if (!(source is FileItemInfo) && string.IsNullOrEmpty (source.LinkTarget))
				throw(new ArgumentException ("can only copy files and symbolic links, recursive operations are not supported by directory service"));
			
			if (string.IsNullOrEmpty (source.LinkTarget)) {
				System.IO.File.Move (source.Path, System.IO.Path.Combine (target.Path, source.Name));
			} else {					
				Syscall.symlink (source.LinkTarget, System.IO.Path.Combine (target.Path, source.Name));
				this.Delete (source);
			}
		}
		
		public void Delete (FileSystemItemInfo item)
		{
			item = this.Reload<FileSystemItemInfo> (item);
			if (!item.CanDelete ())
				throw(new AccessViolationException ("you are not allowed to move <" + item.Path + ">"));
					
			if (item is DirectoryItemInfo && string.IsNullOrEmpty (item.LinkTarget)) {
				System.IO.Directory.Delete (item.Path);
			} else {
				Syscall.unlink (item.Path);
			}
			
			item.SetExpired ();
		}
		
		public void ChangeOwner (FileSystemItemInfo item, uint newUID, uint newGID)
		{
			item = this.Reload< FileSystemItemInfo> (item);
			if (!item.CanChange ())
				throw(new AccessViolationException ("you are not allowed to change <" + item.Path + ">"));
			
			Syscall.chown (item.Path, newUID, newGID);
			
			item.SetExpired ();
		}
		
		public void ChangePermissions (FileSystemItemInfo item, FilePermission userMode, FilePermission groupMode, FilePermission otherMode, bool uidBit, bool gidBit, bool stickyBit)
		{
			item = this.Reload< FileSystemItemInfo> (item);
			if (!item.CanChange ())
				throw(new AccessViolationException ("you are not allowed to change <" + item.Path + ">"));
			
			Stat? stat = new Stat ();
			Stat tmpStat = new Stat ();
			stat = Syscall.stat (item.Path, out tmpStat) == 0 ? tmpStat as Stat? : null;
			
			if (stat.HasValue) {			
				FilePermissions mode = stat.Value.st_mode;
			
				mode = mode ^ FilePermissions.S_IXOTH ^ FilePermissions.S_IWOTH ^ FilePermissions.S_IROTH;
				mode = mode ^ FilePermissions.S_IXGRP ^ FilePermissions.S_IWGRP ^ FilePermissions.S_IRGRP;
				mode = mode ^ FilePermissions.S_IXUSR ^ FilePermissions.S_IWUSR ^ FilePermissions.S_IRUSR;
				mode = mode ^ FilePermissions.S_ISUID ^ FilePermissions.S_ISGID ^ FilePermissions.S_ISVTX;
			
				if ((otherMode & FilePermission.Execute) == FilePermission.Execute)
					mode = mode | FilePermissions.S_IXOTH;
				if ((otherMode & FilePermission.Write) == FilePermission.Write)
					mode = mode | FilePermissions.S_IWOTH;
				if ((otherMode & FilePermission.Read) == FilePermission.Read)
					mode = mode | FilePermissions.S_IROTH;
			
				if ((groupMode & FilePermission.Execute) == FilePermission.Execute)
					mode = mode | FilePermissions.S_IXGRP;
				if ((groupMode & FilePermission.Write) == FilePermission.Write)
					mode = mode | FilePermissions.S_IWGRP;
				if ((groupMode & FilePermission.Read) == FilePermission.Read)
					mode = mode | FilePermissions.S_IRGRP;
			
				if ((userMode & FilePermission.Execute) == FilePermission.Execute)
					mode = mode | FilePermissions.S_IXUSR;
				if ((userMode & FilePermission.Write) == FilePermission.Write)
					mode = mode | FilePermissions.S_IWUSR;
				if ((userMode & FilePermission.Read) == FilePermission.Read)
					mode = mode | FilePermissions.S_IRUSR;
			
				if (uidBit)
					mode = mode | FilePermissions.S_ISUID;
				if (gidBit)
					mode = mode | FilePermissions.S_ISGID;
				if (stickyBit)
					mode = mode | FilePermissions.S_ISVTX;
			
				Syscall.chmod (item.Path, mode);
			
				item.SetExpired ();
				
			} else {
				throw(new ArgumentException ("<" + item.Path + "> is not more existing or accessable"));
			}
		}
		
		private T Reload<T> (T item) where T : FileSystemItemInfo
		{
			item = this.Get (item.Path, 0) as T;
			
			if (item == null)
				throw(new ArgumentException ("<" + item.Path + "> is not more existing or accessable"));
			
			return item;
		}
	}
}

