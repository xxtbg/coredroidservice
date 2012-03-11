using System;
using System.Runtime.Serialization;

namespace CoreDroid.Contract
{
	[DataContract]
	public class ProgressInfo
	{
		[DataMember(Order = 0)]
		public long Current { get; set; }
		
		[DataMember(Order = 1)]
		public long Max { get; set; }
		
		public ProgressInfo ()
		{
			this.Max = 0;
			this.Current = 0;
		}
	}
}

