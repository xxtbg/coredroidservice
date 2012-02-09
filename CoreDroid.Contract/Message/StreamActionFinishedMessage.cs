using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
    public class StreamActionFinishedMessage
    {
        [ProtoMember(1)]
        public bool Success { get; private set; }

        [ProtoMember(2)]
        public string ExceptionAssemblyName { get; private set; }

        [ProtoMember(3)]
        public string ExceptionTypeName { get; private set; }

        [ProtoMember(4)]
        public string ExceptionMessage { get; private set; }

        [ProtoMember(5)]
        public string ExceptionStackTrace { get; private set; }

        public StreamActionFinishedMessage()
        {
            this.Success = true;
        }

        public StreamActionFinishedMessage(Exception ex)
        {
            this.Success = false;
            this.ExceptionAssemblyName = ex.GetType().Assembly.GetName().FullName;
            this.ExceptionTypeName = ex.GetType().FullName;
            this.ExceptionMessage = ex.Message;
            this.ExceptionStackTrace = ex.StackTrace;
        }
    }
}
