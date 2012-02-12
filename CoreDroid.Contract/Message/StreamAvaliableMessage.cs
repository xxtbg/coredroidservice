using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class StreamAvaliableMessage
	{
        [ProtoMember(1)]
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
