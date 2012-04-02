using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
	[DataContract]
	public class InitMessage
	{
		[DataMember(Order = 0)]
		public InitAction Action { get; private set; }
		
		[DataMember(Order = 1)]
		public object Parameter { get; private set; }
		
		private InitMessage ()
		{
		}
		
		public InitMessage (InitAction action)
		{
			this.Action = action;
		}
		
		public InitMessage (InitAction action, object parameter) : this(action)
		{
			this.Parameter = parameter;
		}
	}

	public enum InitAction
	{
		LoadMono,
		//LoadPython,
		Start,
		Stream,
		Close
	}
}
