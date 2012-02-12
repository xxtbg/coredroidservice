using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
    [DataContract]
	public class ServiceRequestMessage
	{
        [DataMember]
		public ServiceRequestAction Action { get; private set; }
		
		private ServiceRequestMessage ()
		{
		}
		
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
