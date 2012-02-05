using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages
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
            this.AssemblyName = type.Assembly.GetName().Name;
            this.TypeName = type.FullName;
        }
    }
}
