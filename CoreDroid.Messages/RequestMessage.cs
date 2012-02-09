using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages
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
