using System;
using System.Runtime.InteropServices;

namespace CoreDroid.Native.Lib.C.Sys
{
	[Flags]
	public enum FileMode : uint
	{
		PermissionOtherExecute	= 0x0001,
		PermissionOtherWrite	= 0x0002,
		PermissionOtherRead		= 0x0004,
		PermissionGroupExecute	= 0x0008,
		PermissionGroupWrite	= 0x0010,
		PermissionGroupRead		= 0x0020,
		PermissionOwnerExecute	= 0x0040,
		PermissionOwnerWrite	= 0x0080,
		PermissionOwnerRead		= 0x0100,
		
		StickyBit	= 0x0200,
		SetGID		= 0x0400,
		SetUID		= 0x0800,
		
		TypeFIFO			= 0x1000,
		TypeCharacterDevice	= 0x2000,
		TypeDirectory		= 0x4000,
		TypeBlockDevice		= 0x6000,
		TypeFile			= 0x8000,
		TypeSymlink			= 0xA000,
		TypeSocket			= 0xC000
		
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct FileStat
	{
		[FieldOffset(0)]
		public ulong ContainingDeviceID;
		[FieldOffset(8)]
		public ulong Inode;
		[FieldOffset(16)]
		public FileMode Mode;
		[FieldOffset(24)]
		public ulong HardLinkCount;
		[FieldOffset(32)]
		public uint UID;
		[FieldOffset(36)]
		public uint GID;
		[FieldOffset(40)]
		public ulong DeviceID;
		[FieldOffset(48)]
		public long Size;
		[FieldOffset(56)]
		public long BlockSize;
		[FieldOffset(64)]
		public long BlockCount;
		[FieldOffset(72)]
		public long LastAccess;
		[FieldOffset(80)]
		public long LastModification;
		[FieldOffset(88)]
		public long LastStatusChange;
		
		public FileStat (byte[] buf)
		{
			this.ContainingDeviceID = BitConverter.ToUInt64 (buf, 0);
			this.Inode = BitConverter.ToUInt64 (buf, 8);
			this.Mode = (FileMode)BitConverter.ToUInt32 (buf, 16);
			this.HardLinkCount = BitConverter.ToUInt64 (buf, 24);
			this.UID = BitConverter.ToUInt32 (buf, 32);
			this.GID = BitConverter.ToUInt32 (buf, 36);
			this.DeviceID = BitConverter.ToUInt64 (buf, 40);
			this.Size = BitConverter.ToInt64 (buf, 48);
			this.BlockSize = BitConverter.ToInt64 (buf, 56);
			this.BlockCount = BitConverter.ToInt64 (buf, 64);
			this.LastAccess = BitConverter.ToInt64 (buf, 72);
			this.LastModification = BitConverter.ToInt64 (buf, 80);
			this.LastStatusChange = BitConverter.ToInt64 (buf, 88);
		}
	}
	
	public static class StatH
	{
		public const string LIBC = "libc";
		
		[DllImport (LIBC, EntryPoint="stat", SetLastError=true)]
		private static extern int _stat (string file, byte[] buf);
		
		public static FileStat? Stat (string file)
		{
			byte[] buf = new byte[96];
			
			int ret = _stat (file, buf);
			
			return ret == 0 ? new FileStat (buf) as FileStat? : null;
		}
		
		[DllImport (LIBC, EntryPoint="stat", SetLastError=true)]
		private static extern int _chmod (string file, uint mode);
		
		public static bool Chmod (string file, FileMode mode)
		{
			return _chmod (file, (uint)mode) == 0;
		}
		
		[DllImport (LIBC, EntryPoint="mknod", SetLastError=true)]
		private static extern int _mknod (string path, uint mode, ulong dev);
		
		public static bool Mknod (string path, FileMode mode, ulong dev)
		{
			return _mknod (path, (uint)mode, dev) == 0;
		}
	}
}

