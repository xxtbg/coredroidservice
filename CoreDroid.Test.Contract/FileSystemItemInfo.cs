using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public abstract class FileSystemItemInfo
	{
		private static ushort? currentUID = null;
		
		protected static ushort CurrentUID {
			get {
				if (currentUID == null) {
					currentUID = Convert.ToUInt16 ("id".Run ("-u"));
				}
				
				return currentUID.Value;
			}
		}
		
		private static IEnumerable<ushort> currentGID = null;
		
		protected static IEnumerable<ushort> CurrentGID {
			get {
				if (currentGID == null) {
					currentGID = "id".Run ("-G").Split (' ').Select (g => Convert.ToUInt16 (g));
				}
				
				return currentGID;
			}
		}
		
		public static FileSystemItemInfo Get (string path)
		{
			if ((System.IO.File.GetAttributes (path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory) {
				return new DirectoryItemInfo (path);
			} else {
				return new FileItemInfo (path);
			}
		}
		
		protected virtual string StatFormat { get { return null; } }
		
		[DataMember(Order = 0)]
		public DateTime LoadTime { get; private set; }
		
		[DataMember(Order = 1)]
		public string Path { get; private set; }
		
		[DataMember(Order = 2)]
		public string DirectoryPath { get ; private set; }
		
		[DataMember(Order = 3)]
		public string Name { get; private set; }
		
		[DataMember(Order = 4)]
		public DateTime AccessTime { get; private set; }

		[DataMember(Order = 5)]
		public DateTime ModifyTime { get; private set; }

		[DataMember(Order = 6)]
		public long Size { get; private set; }
		
		[DataMember(Order = 7)]
		public string LinkTarget { get; private set; }
		
		[DataMember(Order = 8)]
		public string User { get; private set; }
		
		[DataMember(Order = 9)]
		public ushort UID { get; private set; }
		
		[DataMember(Order = 10)]
		public string Group { get; private set; }
		
		[DataMember(Order = 11)]
		public ushort GID{ get; private set; }
		
		[DataMember(Order = 12)]
		public FilePermission UserMode { get; private set; }
		
		[DataMember(Order = 13)]
		public FilePermission GroupMode { get; private set; }
		
		[DataMember(Order = 14)]
		public FilePermission OthersMode { get; private set; }
		
		public FileSystemItemInfo (string path)
		{
			this.LoadTime = DateTime.UtcNow;
			
			System.IO.FileInfo info = new System.IO.FileInfo (path);
			
			this.OnParseInfo (info);
			this.ParseStat (this.Stat ());
		}
		
		protected virtual void OnParseInfo (System.IO.FileInfo info)
		{
			this.Path = info.FullName;
			if (this.Path != "/")
				this.DirectoryPath = info.DirectoryName;
			this.Name = info.Name;
			this.AccessTime = info.LastAccessTime;
			this.ModifyTime = info.LastWriteTime;
		}

		/*
		 File: „/“
  Size: 4096      	Blocks: 8          IO Block: 4096   Verzeichnis
Device: 802h/2050d	Inode: 2           Links: 22
Access: (0755/drwxr-xr-x)  Uid: (    0/    root)   Gid: (    0/    root)
Access: 2012-03-07 18:37:48.324771274 +0100
Modify: 2012-03-06 18:37:40.912285837 +0100
Change: 2012-03-06 18:37:40.912285837 +0100*/
		
		private void ParseStat (string statOutput)
		{
			Queue<string> statInfos = new Queue<string> ();
			foreach (string statInfo in statOutput.Split ('|'))
				statInfos.Enqueue (statInfo);
			
			string[] fileNameParts = statInfos.Dequeue ().Split (new string[]{"\" -> \""}, StringSplitOptions.None);
			this.LinkTarget = fileNameParts.Length > 1 ? fileNameParts [1].Remove (fileNameParts [1].Length - 1) : null;
			
			this.Size = Convert.ToInt64 (statInfos.Dequeue ());
			
			string accessString = statInfos.Dequeue ();
			this.UserMode = (FilePermission)Convert.ToUInt16 (accessString [0]);
			this.GroupMode = (FilePermission)Convert.ToUInt16 (accessString [1]);
			this.OthersMode = (FilePermission)Convert.ToUInt16 (accessString [2]);
			
			this.UID = Convert.ToUInt16 (statInfos.Dequeue ());
			this.User = statInfos.Dequeue ();
			this.GID = Convert.ToUInt16 (statInfos.Dequeue ());
			this.Group = statInfos.Dequeue ();
			
			this.OnParseStat (statInfos);
		}
		
		protected virtual void OnParseStat (Queue<string> statInfos)
		{
		}
		
		private string Stat ()
		{			
			try {
				return "stat".Run ("-c \"%N|%s|%a|%u|%U|%g|%G" + (!string.IsNullOrEmpty (this.StatFormat) ? "|" + this.StatFormat : string.Empty) + "\"", this.Path);				
			} catch (Exception ex) {
				throw(new ArgumentException ("could not stat <" + this.Path + ">\n" + ex.Message));
			}	
		}
		
		protected bool HasPermission (FilePermission mode)
		{
			return CurrentUID == 0 ||
				((this.OthersMode & mode) == mode) ||
				((this.GroupMode & mode) == mode && CurrentGID.Contains (this.GID)) ||
				((this.UserMode & mode) == mode && CurrentUID == this.UID);	
		}
		
		public bool CanRead ()
		{
			return this.HasPermission (FilePermission.Read);
		}
		
		public bool CanWrite ()
		{
			return this.HasPermission (FilePermission.Write);
		}
		
		public bool CanExecute ()
		{
			return this.HasPermission (FilePermission.Execute);
		}
		
		public bool CanCopy ()
		{
			return this.CanRead ();
		}
		
		public bool CanMove ()
		{
			return this.CanRead () && this.CanWrite ();
		}
		
		public bool CanDelete ()
		{
			return this.CanWrite ();
		}
		
		public bool CanChange ()
		{
			return this.CanWrite ();
		}
		
		private void InternalLink (DirectoryItemInfo target)
		{
			"ln".Run ("-s", this.LinkTarget, System.IO.Path.Combine (target.Path, this.Name));
		}
		
		protected virtual void InternalCopy (DirectoryItemInfo target)
		{
			"cp".Run ("-fr", System.IO.Path.Combine (target.Path, this.Name));
		}
		
		protected virtual void InternalMove (DirectoryItemInfo target)
		{
			"mv".Run (this.Path, System.IO.Path.Combine (target.Path, this.Name));
		}
		
		private void InternalDeleteLink ()
		{
			"rm".Run (this.Path);
		}
		
		protected virtual void InternalDelete ()
		{	
			"rm".Run ("-fr", this.Path);
		}
		
		protected virtual void InternalChangeOwner (ushort newUID, ushort newGID)
		{
			"chown".Run (newUID.ToString () + ":" + newGID.ToString (), this.Path);
		}
		
		protected virtual void InternalChangeMode (ushort newMode)
		{
			"chmod".Run ("0" + newMode.ToString (), this.Path);
		}
		
		public void Copy (DirectoryItemInfo target)
		{
			if (!this.CanCopy ())
				throw(new AccessViolationException ("you are not allowed to copy <" + this.Path + ">"));
			
			if (string.IsNullOrEmpty (this.LinkTarget)) {
				this.InternalCopy (target);
			} else {					
				this.InternalLink (target);
			}
		}
		
		public void Move (DirectoryItemInfo target)
		{
			if (!this.CanMove ())
				throw(new AccessViolationException ("you are not allowed to move <" + this.Path + ">"));
			
			if (string.IsNullOrEmpty (this.LinkTarget)) {
				this.InternalMove (target);
			} else {					
				this.InternalLink (target);
				this.InternalDeleteLink ();				
			}
		}
		
		public void Delete ()
		{
			if (!this.CanDelete ())
				throw(new AccessViolationException ("you are not allowed to move <" + this.Path + ">"));
			
			if (string.IsNullOrEmpty (this.LinkTarget)) {
				this.InternalDelete ();
			} else {					
				this.InternalDeleteLink ();
			}
			
			this.LoadTime = DateTime.MinValue;
		}
		
		public void ChangeOwner (ushort newUID, ushort newGID)
		{
			if (!this.CanChange ())
				throw(new AccessViolationException ("you are not allowed to change <" + this.Path + ">"));
			
			this.InternalChangeOwner (newUID, newGID);
		}
		
		public void ChangeMode (ushort newMode)
		{
			if (!this.CanChange ())
				throw(new AccessViolationException ("you are not allowed to change <" + this.Path + ">"));
			
			this.InternalChangeMode (newMode);
		}
	}
	
	[DataContract]
	public class DirectoryItemInfo : FileSystemItemInfo
	{
		[DataMember(Order = 0)]
		public string MountDevice { get; private set; }
		
		public DirectoryItemInfo (string path):base(path)
		{
		}
		
		protected override void InternalDelete ()
		{
			System.IO.Directory.Delete (this.Path);
		}
	}
	
	[DataContract]
	public class FileItemInfo : FileSystemItemInfo
	{		
		[DataMember(Order = 0)]
		public string Extension { get; private set; }
		
		[DataMember(Order = 1)]
		public bool IsBlockDevice { get; private set; }
		
		[DataMember(Order = 2)]
		public bool IsCharacterDevice { get; private set; }

		public FileItemInfo (string path) : base(path)
		{
		}
		
		protected override void OnParseInfo (System.IO.FileInfo info)
		{
			base.OnParseInfo (info);
			
			this.Extension = info.Extension;			
		}
		
		protected override void InternalCopy (DirectoryItemInfo target)
		{
			System.IO.File.Copy (this.Path, System.IO.Path.Combine (this.Path, System.IO.Path.Combine (target.Path, this.Name)));
		}
		
		protected override void InternalMove (DirectoryItemInfo target)
		{
			System.IO.File.Move (this.Path, System.IO.Path.Combine (this.Path, System.IO.Path.Combine (target.Path, this.Name)));
		}
		
		protected override void InternalDelete ()
		{
			System.IO.File.Delete (this.Path);
		}
	}
	
	[Flags]
	public enum FilePermission : ushort
	{
		Execute = 0x01,
		Write = 0x02,
		Read = 0x04
	}
}

