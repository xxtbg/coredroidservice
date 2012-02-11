using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CoreDroid.Contract.Message
{
    [ProtoContract]
	public class InitMessage
	{
        [ProtoMember(1)]
		public InitAction Action { get; private set; }
	}

	public enum InitAction
	{
		LoadMono,
		//AddPythonPlugin,
		Start,
		Stream
	}
}
