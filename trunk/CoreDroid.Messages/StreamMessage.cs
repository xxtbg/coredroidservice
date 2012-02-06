using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages
{
    [ProtoContract]
    public class StreamMessage
    {
        [ProtoMember(1)]
        public int Id { get; private set; }

        public StreamMessage(int id)
        {
            this.Id = id;
        }
    }
}
