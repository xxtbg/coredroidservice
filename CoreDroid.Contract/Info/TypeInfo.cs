using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System.Reflection;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
	[DataContract]
	public class TypeInfo
	{
		[DataMember(Order = 0)]
		public bool IsNull { get; private set; }
		
		[DataMember(Order = 1)]
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
		
		[DataMember(Order = 2)]
		public bool IsArrayType { get; private set; }
		
		[DataMember(Order = 3)]
		public string TypeName { get; private set; }
        
		private bool typeSearched = false;
		private Type type;

		public Type Type {
			get {
				if (!this.typeSearched) {
					this.type = this.Assembly != null ? this.Assembly.GetTypes ().Where (t => t.FullName == this.TypeName || (t.FullName.StartsWith (this.TypeName + "`") && t.GetGenericArguments ().Length == this.GenericArguments.Length)).FirstOrDefault () : null;
					this.typeSearched = true;
				}

				return this.type;
			}
		}
		
		[DataMember(Order = 4)]
		public TypeInfo[] GenericArguments;
		
		private TypeInfo ()
		{
		}
		
		public TypeInfo (string assemblyName, string typeName)
		{
			this.IsNull = false;
			this.AssemblyName = assemblyName;
			this.IsArrayType = false;
			this.TypeName = typeName;
			this.GenericArguments = null;
		}
		
		public TypeInfo (Type type)
		{
			if (type != null) {
				this.IsNull = false;
				this.AssemblyName = type.Assembly.GetName ().Name;
				this.IsArrayType = type.FullName.EndsWith ("[]");
				this.TypeName = this.IsArrayType ? type.FullName.Remove (type.FullName.Length - 2) : type.FullName;
				
				if (type.IsGenericType) {
					this.GenericArguments = type.GetGenericArguments ().Select (t => new TypeInfo (t)).ToArray ();
					this.TypeName = this.TypeName.Remove (this.TypeName.IndexOf ('`'));
				}
			} else {
				this.IsNull = true;
			}
		}
	}
}
