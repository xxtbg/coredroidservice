using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class ServiceRequestMessage
	{
        [ProtoMember(1)]
		public ServiceRequestAction Action { get; private set; }
		
		public ServiceRequestMessage (ServiceRequestAction action)
		{
			this.Action = action;
		}
	}

	public enum ServiceRequestAction
	{
		Close,
		Call
	}
}
