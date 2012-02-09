using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Reflection;

namespace CoreDroid.Messages
{
    [ProtoContract]
    public class ServiceCallMessage
    {
        [ProtoMember(1)]
        public string MethodName { get; private set; }

        [ProtoMember(2)]
        public MethodParameterInfo[] MethodParameterInfos { get; private set; }

        public ServiceCallMessage(string methodName, MethodParameterInfo[] methodParameterInfos)
        {
            this.MethodName = methodName;
            this.MethodParameterInfos = methodParameterInfos;
        }
    }

    [ProtoContract]
    public class MethodParameterInfo
    {
        [ProtoMember(1)]
        public string Name { get; private set; }

        [ProtoMember(2)]
        public string AssemblyName { get; private set; }

        private bool assemblySearched = false;
        private Assembly assembly;
        public Assembly Assembly
        {
            get
            {
                if (!this.assemblySearched)
                {
                    this.assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().FullName == this.AssemblyName).FirstOrDefault();
                    this.assemblySearched = true;
                }

                return this.assembly;
            }
        }

        [ProtoMember(3)]
        public string TypeName { get; private set; }

        private bool typeSearched = false;
        private Type type;
        public Type Type
        {
            get
            {
                if (!this.typeSearched)
                {
                    this.type = this.Assembly != null ? this.Assembly.GetTypes().Where(t => t.Name == this.TypeName).FirstOrDefault() : null;
                    this.typeSearched = true;
                }

                return this.type;
            }
        }

        public MethodParameterInfo(string name, Type type)
        {
            this.Name = name;
            this.AssemblyName = type.Assembly.GetName().FullName;
            this.TypeName = type.FullName;
        }
    }
}
