using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Reflection;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class TypeMessage
	{
		[ProtoMember(1)]
		public bool IsNull { get; private set; }
		
        [ProtoMember(2)]
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

        [ProtoMember(3)]
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

		public TypeMessage (string assemblyName, string typeName)
		{
			this.IsNull = false;
			this.AssemblyName = assemblyName;
			this.TypeName = typeName;
		}
		
		public TypeMessage (Type type)
		{
			this.IsNull = false;
			this.AssemblyName = type.Assembly.GetName ().Name;
			this.TypeName = type.FullName;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CoreDroid.Contract.Message.TypeMessage"/> class.
		/// Call empty constructor for null
		/// </summary>
		public TypeMessage ()
		{
			this.IsNull = true;
		}
	}
}
