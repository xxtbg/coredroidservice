using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract
{
	[DataContract]
	public class ExceptionInfo
	{
		[DataMember(Order = 0)]
		public string ExceptionAssemblyName { get; private set; }

		[DataMember(Order = 1)]
		public string ExceptionTypeName { get; private set; }

		[DataMember(Order = 2)]
		public string ExceptionMessage { get; private set; }

		[DataMember(Order = 3)]
		public string ExceptionStackTrace { get; private set; }
		
		[DataMember(Order = 4)]
		public ExceptionInfo Inner { get; private set; }

		public ExceptionInfo (Exception ex)
		{
			this.ExceptionAssemblyName = ex.GetType ().Assembly.GetName ().FullName;
			this.ExceptionTypeName = ex.GetType ().FullName;
			this.ExceptionMessage = ex.Message;
			this.ExceptionStackTrace = ex.StackTrace;
			
			if (ex.InnerException != null)
				this.Inner = new ExceptionInfo (ex.InnerException);
		}
	}
}
