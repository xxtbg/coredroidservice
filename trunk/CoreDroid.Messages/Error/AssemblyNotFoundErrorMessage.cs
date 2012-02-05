using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages.Error
{
    [ProtoContract]
    public class AssemblyNotFoundErrorMessage
    {
        [ProtoMember(1)]
        public string AssemblyName { get; private set; }

        public AssemblyNotFoundErrorMessage(string assemblyName)
        {
            this.AssemblyName = assemblyName;
        }
    }
}
