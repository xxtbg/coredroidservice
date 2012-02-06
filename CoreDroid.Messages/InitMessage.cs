using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages
{
    [ProtoContract]
    public class InitMessage
    {
        [ProtoMember(1)]
        public InitAction Action { get; private set; }
    }

    public enum InitAction
    {
        AddDllPlugin,
        //AddPythonPlugin,
        Start,
        Stream
    }
}
