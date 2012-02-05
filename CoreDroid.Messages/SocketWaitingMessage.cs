using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid
{
    [ProtoContract]
    public class SocketWaitingMessage
    {
        [ProtoMember(1)]
        public int Id { get; private set; }

        public SocketWaitingMessage(int id)
        {
            this.Id = id;
        }
    }
}
