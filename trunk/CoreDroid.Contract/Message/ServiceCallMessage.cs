using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System.Reflection;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
	[DataContract]
	public class ServiceCallMessage
	{
		[DataMember(Order = 0)]
		public string ChildName { get; private set; }

		[DataMember(Order = 1)]
		public TypeInfo[] Parameter { get; private set; }
		
		private ServiceCallMessage ()
		{
		}
		
		public ServiceCallMessage (string childName, TypeInfo[] parameterInfos)
		{
			this.ChildName = childName;
			this.Parameter = parameterInfos;
		}
	}
}
