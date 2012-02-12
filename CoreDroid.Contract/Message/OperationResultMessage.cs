using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class OperationResultMessage
	{
        [ProtoMember(1)]
		public bool Success { get; private set; }

        [ProtoMember(2)]
		public ExceptionInfo Exception { get; private set; }
		
		public OperationResultMessage ()
		{
			this.Success = true;
		}

		public OperationResultMessage (Exception ex)
		{
			this.Success = false;
			this.Exception = new ExceptionInfo (ex);
		}
	}
}
