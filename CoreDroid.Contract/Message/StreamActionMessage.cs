using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CoreDroid.Contract.Message
{
	[DataContract]
	public class StreamActionMessage
	{
		[DataMember(Order = 0)]
		public StreamAction Action { get; private set; }
		
		[DataMember(Order = 1)]
		public long Position { get; private set; }

		[DataMember(Order = 2)]
		public long Offset { get; private set; }

		[DataMember(Order = 3)]
		public long Size { get; private set; }
		
		private StreamActionMessage ()
		{
		}
		
		public StreamActionMessage (StreamAction action) : this(action, 0, 0, 0)
		{
		}

		public StreamActionMessage (StreamAction action, long position, long offset, long size)
		{
			this.Action = action;
			this.Position = position;
			this.Offset = offset;
			this.Size = size;
		}
	}

	public enum StreamAction
	{
		CanRead,
		CanSeek,
		CanWrite,
		Length,
		GetPosition,
		SetPosition,
		Flush,
		Read,
		Seek,
		SetLength,
		Write,
		Close
	}
}
