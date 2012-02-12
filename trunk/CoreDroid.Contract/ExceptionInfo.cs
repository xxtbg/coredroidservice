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
        [DataMember]
		public string ExceptionAssemblyName { get; private set; }

        [DataMember]
		public string ExceptionTypeName { get; private set; }

        [DataMember]
		public string ExceptionMessage { get; private set; }

        [DataMember]
		public string ExceptionStackTrace { get; private set; }

		public ExceptionInfo (Exception ex)
		{
			this.ExceptionAssemblyName = ex.GetType ().Assembly.GetName ().FullName;
			this.ExceptionTypeName = ex.GetType ().FullName;
			this.ExceptionMessage = ex.Message;
			this.ExceptionStackTrace = ex.StackTrace;
		}
	}
}
