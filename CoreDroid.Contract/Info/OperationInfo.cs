using System;
using System.Runtime.Serialization;

namespace CoreDroid.Contract
{
	[DataContract]
	public class OperationInfo
	{
		private static int lastID = 0;
		
		[DataMember(Order = 0)]
		public int ID{ get; private set; }
		
		[DataMember(Order = 1)]
		public ExceptionInfo Exception { get; set; }
		
		[DataMember(Order = 2)]
		public ProgressInfo OverallProgress { get; set; }
		
		[DataMember(Order = 3)]
		public bool IsFinished { get; set; }
		
		public bool IsRunning {
			get {
				return !this.IsFinished && this.Exception == null;
			}
		}
		
		public OperationInfo ()
		{
			this.ID = lastID++;
			this.IsFinished = false;
			this.OverallProgress = new ProgressInfo ();
		}
	}
}

