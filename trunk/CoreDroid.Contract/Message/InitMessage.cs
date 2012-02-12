﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
    [DataContract]
	public class InitMessage
	{
        [DataMember]
		public InitAction Action { get; private set; }
		
		private InitMessage ()
		{
		}
		
		public InitMessage (InitAction action)
		{
			this.Action = action;
		}
	}

	public enum InitAction
	{
		LoadMono,
		//AddPythonPlugin,
		Start,
		Stream
	}
}
