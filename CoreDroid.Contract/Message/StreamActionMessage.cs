using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
    public class StreamActionMessage
    {
        [ProtoMember(1)]
        public StreamAction Action { get; private set; }

        [ProtoMember(2)]
        public long Position { get; private set; }

        [ProtoMember(3)]
        public long Offset { get; private set; }

        [ProtoMember(4)]
        public long Size { get; private set; }

        public StreamActionMessage(StreamAction action) : this(action, 0, 0, 0) { }

        public StreamActionMessage(StreamAction action, long position, long offset, long size)
        {
            this.Action = action;
            this.Position = position;
            this.Offset = offset;
            this.Size = size;
        }
    }

    public enum StreamAction
    {
        CanRead,
        CanSeek,
        CanWrite,
        Length,
        GetPosition,
        SetPosition,
        Flush,
        Read,
        Seek,
        SetLength,
        Write,
        Close
    }
}
