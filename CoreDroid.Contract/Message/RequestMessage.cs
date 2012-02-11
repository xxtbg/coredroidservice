using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class RequestMessage
	{
        [ProtoMember(1)]
		public RequestAction Action { get; private set; }
	}

	public enum RequestAction
	{
		Close,
		Call
	}
}
