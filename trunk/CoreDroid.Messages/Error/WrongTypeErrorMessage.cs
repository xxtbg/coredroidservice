using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Messages.Error
{
    [ProtoContract]
    public class WrongTypeErrorMessage
    {
        [ProtoMember(1)]
        public string AssemblyName { get; private set; }

        [ProtoMember(2)]
        public string TypeName { get; private set; }

        [ProtoMember(3)]
        public string ErrorMessage { get; private set; }

        public WrongTypeErrorMessage(Type type, string errorMessage)
        {
            this.AssemblyName = type.Assembly.GetName().Name;
            this.TypeName = type.FullName;
            this.ErrorMessage = errorMessage;
        }
    }
}
