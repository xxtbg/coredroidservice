﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
	[DataContract]
	public class OperationResultMessage
	{
		[DataMember(Order = 0)]
		public bool Success { get; private set; }

		[DataMember(Order = 1)]
		public ExceptionInfo Exception { get; private set; }
		
		private OperationResultMessage ()
		{
		}
		
		public OperationResultMessage (bool success)
		{
			this.Success = success;
		}

		public OperationResultMessage (Exception ex)
		{
			this.Success = false;
			this.Exception = new ExceptionInfo (ex);
		}
	}
}
