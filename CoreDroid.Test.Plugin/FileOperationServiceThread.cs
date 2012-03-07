using System;
using CoreDroid.Contract;

namespace DiskDroid.FileSystem
{
	public class FileOperationServiceThread : OperationServiceThread
	{
		public FileOperationServiceThread (FileOperationInfo info) : base(info, this.Worker)
		{
		}
		
		private void Worker ()
		{
			// TODO Work on
		}
	}
}

