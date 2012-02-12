using System;

namespace CoreDroid.Contract
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ServiceContractAttribute : Attribute
	{
		public ServiceContractAttribute ()
		{
		}
	}
	
	[AttributeUsage(AttributeTargets.Method)]
	public class ServiceMemberAttribute : Attribute
	{
		public ServiceMemberAttribute ()
		{
		}
	}
}

