using System;
using CoreDroid.Contract;

using DiskDroid.FileSystem.Contract;
using System.Threading;

namespace DiskDroid.FileSystem
{
	public class FileOperationServiceThread : OperationServiceThread
	{
		protected override ThreadStart Worker { get { return new ThreadStart (this.FileOperationWorker); } }
		
		public FileOperationServiceThread (FileOperationInfo info) : base(info)
		{
		}
		
		private void FileOperationWorker ()
		{
			// TODO Work on
		}
	}
}

