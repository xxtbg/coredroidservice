using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages.Error
{
    [ProtoContract]
    public class TypeNotFoundErrorMessage
    {
        [ProtoMember(1)]
        public string AssemblyName { get; private set; }

        [ProtoMember(2)]
        public string TypeName { get; private set; }

        public TypeNotFoundErrorMessage(string assemblyName, string typeName)
        {
            this.AssemblyName = assemblyName;
            this.TypeName = typeName;
        }
    }
}
