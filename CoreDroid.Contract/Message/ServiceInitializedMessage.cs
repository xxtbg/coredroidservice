using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
    public class ServiceInitializedMessage
    {
        [ProtoMember(1)]
        public string AssemblyName { get; private set; }

        [ProtoMember(2)]
        public string TypeName { get; private set; }

        public ServiceInitializedMessage(Type type)
        {
            this.AssemblyName = type.Assembly.GetName().FullName;
            this.TypeName = type.FullName;
        }
    }
}
