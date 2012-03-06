using System;
using CoreDroid.Contract;
using System.Runtime.Serialization;

namespace DiskDroid.FileSystem.Contract
{
	[DataContract]
	public class OperationInfo
	{
		private static int lastID = 0;
		
		[DataMember]
		public int ID{ get; private set; }
		
		[DataMember]
		public ExceptionInfo Exception { get; set; }
		
		[DataMember]
		public bool IsFinished { get; set; }
		
		public OperationInfo ()
		{
			this.ID = lastID++;
			this.IsFinished = false;
		}
	}
	
	[DataContract]
	public class FileOperationInfo : OperationInfo
	{
		[DataMember]
		public string ActualPath { get; set; }
		
		[DataMember]
		public FileProgressInfo Progress { get; private set; }
		
		public FileOperationInfo (long operationSize) : base()
		{
			this.Progress = new FileProgressInfo (operationSize);
		}
	}
	
	[DataContract]
	public class FileProgressInfo
	{
		[DataMember]
		public long Current { get; set; }
		
		[DataMember]
		public long Max { get; private set; }
		
		public FileProgressInfo (long max)
		{
			this.Max = max;
		}
	}
}

