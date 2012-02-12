using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

using CoreDroid.Extensions;
using CoreDroid.Contract.Message;
using CoreDroid.Contract;

namespace CoreDroid
{
	public class ServiceStream : Stream, IDisposable
	{
		private TcpClient client;
		private NetworkStream netStream;

		internal ServiceStream (TcpClient client)
		{
			this.client = client;
			this.netStream = client.GetStream ();
		}

		public override bool CanRead {
			get { return this.DoStreamAction<bool> (StreamAction.CanRead); }
		}

		public override bool CanSeek {
			get { return this.DoStreamAction<bool> (StreamAction.CanSeek); }
		}

		public override bool CanWrite {
			get { return this.DoStreamAction<bool> (StreamAction.CanWrite); }
		}

		public override void Flush ()
		{
			this.DoStreamAction (StreamAction.Flush);
		}

		public override long Length {
			get { return this.DoStreamAction<long> (StreamAction.Length); }
		}

		public override long Position {
			get {
				return this.DoStreamAction<long> (StreamAction.GetPosition);
			}
			set {
				this.DoStreamAction (StreamAction.SetPosition, null, value, 0, 0);
			}
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int read = this.DoStreamAction<int> (StreamAction.Read, null, 0, 0, count);
			this.netStream.Read (buffer, offset, read);
			return read;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			// message.Position == 0 ? SeekOrigin.Begin : (message.Position == 1 ? SeekOrigin.Current : SeekOrigin.End)
			return this.DoStreamAction<long> (StreamAction.Seek, null, (origin == SeekOrigin.Begin ? 0 : (origin == SeekOrigin.Current ? 1 : 2)), offset, 0);
		}

		public override void SetLength (long value)
		{
			this.DoStreamAction (StreamAction.SetLength, null, 0, 0, value);
		}

		public override void Write (byte[] buffer, int offset, int size)
		{
			this.DoStreamAction (StreamAction.Write, buffer, 0, offset, size);
		}

		public override void Close ()
		{
			this.DoStreamAction (StreamAction.Close);
			this.netStream.Close ();
			this.client.Close ();
		}
		
		protected override void Dispose (bool disposing)
		{
			try {
				if (disposing)
					this.Close ();
			} catch {
			}
			
			base.Dispose ();
			
			base.Dispose (disposing);
		}

		private void DoStreamAction (StreamAction action)
		{
			this.DoStreamAction (action, null, 0, 0, 0);
		}

		private T DoStreamAction<T> (StreamAction action)
		{
			return this.DoStreamAction<T> (action, null, 0, 0, 0);
		}

		private void DoStreamAction (StreamAction action, byte[] buffer, long position, long offset, long size)
		{
			this.DoStreamAction<object> (action, buffer, position, offset, size);
		}

		private T DoStreamAction<T> (StreamAction action, byte[] buffer, long position, long offset, long size)
		{
			this.netStream.ProtoSend (new StreamActionMessage (action, position, action != StreamAction.Write ? offset : 0, size));

			if (action == StreamAction.Write)
				this.netStream.Write (buffer, Convert.ToInt32 (offset), Convert.ToInt32 (size));

			OperationResultMessage msg = this.netStream.ProtoReceive<OperationResultMessage> ();

			if (!msg.Success)
				throw(new ServiceException (msg));

			return this.netStream.ProtoReceive<T> ();
		}
	}
}
