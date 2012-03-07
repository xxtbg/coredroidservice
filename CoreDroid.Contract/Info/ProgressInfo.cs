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
		public long Max { get; private set; }
		
		public ProgressInfo (long max)
		{
			this.Max = max;
		}
	}
}

