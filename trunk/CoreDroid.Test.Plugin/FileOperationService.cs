using System;
using System.Runtime.Serialization;
using CoreDroid.Contract;
using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	public class FileOperationService : OperationService<FileOperationServiceThread, FileOperationInfo>
	{
		public int Start ()
		{// TODO todo
			this.Create (new FileOperationInfo (111));
			
			return 0;
		}
	}
}
