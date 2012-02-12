using System;
using CoreDroid.Contract.Message;

namespace CoreDroid.Contract
{
	public class ServiceException : Exception
	{
		public string OriginalAssemblyName { get; private set; }

		public string OriginalTypeName { get; private set; }

		public string OriginalMessage { get; private set; }

		public string OriginalStackTrace { get; private set; }

		public ServiceException (OperationResultMessage msg)
            : base()
		{
			this.OriginalAssemblyName = msg.Exception.ExceptionAssemblyName;
			this.OriginalTypeName = msg.Exception.ExceptionTypeName;
			this.OriginalMessage = msg.Exception.ExceptionMessage;
			this.OriginalStackTrace = msg.Exception.ExceptionStackTrace;
		}
	}
}

