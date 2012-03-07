using System;
using CoreDroid.Contract;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public class FileOperationInfo : OperationInfo
	{
		private FileSystemItemInfo actual;

		[DataMember(Order = 0)]
		public FileSystemItemInfo Actual {
			get {
				return this.actual;
			}
			set {
				this.actual = value;
				this.ActualProgress = new ProgressInfo (value.Size);
			}
		}
		
		[DataMember(Order = 1)]
		public ProgressInfo OverallProgress { get; private set; }
		
		[DataMember(Order = 2)]
		public ProgressInfo ActualProgress { get; private set; }
		
		public FileOperationInfo (long overallProgressSize) : base()
		{
			this.OverallProgress = new ProgressInfo (overallProgressSize);
		}
	}
}

