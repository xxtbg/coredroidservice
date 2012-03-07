using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
	[DataContract]
	public class StreamAvaliableMessage
	{
		[DataMember(Order = 0)]
		public int Id { get; private set; }
		
		private StreamAvaliableMessage ()
		{
		}
		
		public StreamAvaliableMessage (int id)
		{
			this.Id = id;
		}
	} 
}
