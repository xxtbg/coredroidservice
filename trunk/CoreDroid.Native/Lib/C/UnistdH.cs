using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CoreDroid.Native.Lib.C
{
	[Flags]
	public enum FileAccessType : int
	{
		Exists = 0,
		Execute = 1,
		Write = 2,
		Read = 4
	}
	
	public static class UnistdH
	{
		public const string LIBC = "libc";
		
		#region user
		[DllImport (LIBC, EntryPoint="getuid", SetLastError=true)]
		public static extern int GetUID ();

		[DllImport (LIBC, EntryPoint="getgid", SetLastError=true)]
		public static extern int GetGID ();

		[DllImport (LIBC, EntryPoint="getgroups", SetLastError=true)]
		private static extern int _getgroups (int size, int[] list);
		
		public static int[] GetGroups ()
		{
			int[] buf = new int[32];
			int amount = _getgroups (32, buf);
			
			int[] ret = new int[amount];
			Array.Copy (buf, ret, amount);
			return ret;
			
		}
		
		[DllImport (LIBC, EntryPoint="group_member", SetLastError=true)]
		private static extern int _group_member (int gid);
		
		public static bool IsGroupMember (int gid)
		{
			return _group_member (gid) != 0;
		}
		#endregion
		
		#region file io
		[DllImport (LIBC, EntryPoint="access", SetLastError=true)]
		private extern static int _access (string name, int type);
		
		public static bool Access (string name, FileAccessType type)
		{
			return _access (name, (int)type) == 0;
		}
		
		[DllImport (LIBC, EntryPoint="chown", SetLastError=true)]
		private static extern int _chown (string file, int owner, int group);
		
		public static bool Chown (string file, int owner, int group)
		{
			return _chown (file, owner, group) == 0;
		}
		
		[DllImport (LIBC, EntryPoint="symlink", SetLastError=true)]
		private static extern int _symlink (string from, string to);
		
		public static bool Symlink (string from, string to)
		{
			return _symlink (from, to) == 0;
		}
		
		[DllImport (LIBC, EntryPoint="readlink", SetLastError=true)]
		private static extern int _readlink (string path, byte[] buf, int len);
		
		public static string Readlink (string path)
		{
			byte[] buf = new byte[512];
			int amount = _readlink (path, buf, 512);
			
			if (amount < 0)
				return null;
			else
				return Encoding.Default.GetString (buf, 0, amount);
		}
		
		[DllImport (LIBC, EntryPoint="unlink", SetLastError=true)]
		private static extern int _unlink (string name);
		
		public static bool Unlink (string name)
		{
			return _unlink (name) == 0;
		}
		#endregion
		
		#region system
		[DllImport (LIBC, EntryPoint="gethostname", SetLastError=true)]
		private static extern int _gethostname (byte[] buf, int len);
		
		public static string GetHostname ()
		{
			byte[] buf = new byte[256];
			int amount = _gethostname (buf, 256);
			
			if (amount < 0)
				return null;
			else
				return Encoding.Default.GetString (buf, 0, amount);
		}

		[DllImport (LIBC, EntryPoint="getdomainname", SetLastError=true)]
		private static extern int _getdomainname (byte[] buf, int len);
		
		public static string GetDomainname ()
		{
			byte[] buf = new byte[256];
			int amount = _getdomainname (buf, 256);
			
			if (amount < 0)
				return null;
			else
				return Encoding.Default.GetString (buf, 0, amount);
		}
		
		[DllImport (LIBC, EntryPoint="chroot", SetLastError=true)]
		private static extern int _chroot (string path);
		
		public static bool Chroot (string path)
		{
			return _chroot (path) == 0;
		}
		#endregion
	}
}

