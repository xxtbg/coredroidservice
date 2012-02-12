using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Reflection;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class ServiceReturnMessage
	{
        [ProtoMember(1)]
		public string AssemblyName { get; private set; }
		
		private bool assemblySearched = false;
		private Assembly assembly;

		public Assembly Assembly {
			get {
				if (!this.assemblySearched) {
					this.assembly = AppDomain.CurrentDomain.GetAssemblies ().Where (a => a.GetName ().Name == this.AssemblyName).FirstOrDefault ();
					this.assemblySearched = true;
				}

				return this.assembly; 
			}
		}

        [ProtoMember(2)]
		public string TypeName { get; private set; }
        
		private bool typeSearched = false;
		private Type type;

		public Type Type {
			get {
				if (!this.typeSearched) {
					this.type = this.Assembly != null ? this.Assembly.GetTypes ().Where (t => t.Name == this.TypeName).FirstOrDefault () : null;
					this.typeSearched = true;
				}

				return this.type;
			}
		}

		public ServiceReturnMessage (Type type)
		{
			if (type != null) {
				this.AssemblyName = type.Assembly.GetName ().Name;
				this.TypeName = type.FullName;
			}
		}
	}
}
