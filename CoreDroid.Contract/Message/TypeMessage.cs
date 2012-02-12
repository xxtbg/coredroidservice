using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System.Reflection;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
    [DataContract]
	public class TypeMessage
	{
		[DataMember]
		public bool IsNull { get; private set; }
		
        [DataMember]
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

        [DataMember]
		public string TypeName { get; private set; }
        
		private bool typeSearched = false;
		private Type type;

		public Type Type {
			get {
				if (!this.typeSearched) {
					this.type = this.Assembly != null ? this.Assembly.GetTypes ().Where (t => t.FullName == this.TypeName).FirstOrDefault () : null;
					this.typeSearched = true;
				}

				return this.type;
			}
		}
		
		private TypeMessage ()
		{
		}
		
		public TypeMessage (string assemblyName, string typeName)
		{
			this.IsNull = false;
			this.AssemblyName = assemblyName;
			this.TypeName = typeName;
		}
		
		public TypeMessage (Type type)
		{
			if (type != null) {
				this.IsNull = false;
				this.AssemblyName = type.Assembly.GetName ().Name;
				this.TypeName = type.FullName;
			} else {
				this.IsNull = true;
			}
		}
	}
}
