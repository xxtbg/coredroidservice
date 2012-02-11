using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.IO;

using CoreDroid.Extensions;
using System.Net;
using CoreDroid.Contract.Message;
using CoreDroid.Contract;

namespace CoreDroid
{
	public class SocketClient : IDisposable
	{
		public  int Port { get; private set; }

		public SocketClient (int port)
		{
			this.Port = port;
		}
		
		public void LoadMono (Stream dllStream)
		{
			TcpClient client = new TcpClient ();
			client.Connect (IPAddress.Loopback, this.Port);
			NetworkStream stream = client.GetStream ();
			
			stream.ProtoSend (new InitMessage (InitAction.LoadMono));
			
			using (ServiceStream destStream = this.GetStream(stream)) {
				dllStream.CopyTo (destStream);
			}
			
			stream.ProtoSend (new SendingPluginFinishedMessage ());
			OperationFinishedMessage msg = stream.ProtoReceive<OperationFinishedMessage> ();
			
			if (!msg.Success)
				throw(new ServiceException (msg));
		}
		
		private ServiceStream GetStream (NetworkStream stream)
		{
			StreamAvaliableMessage avalMsg = stream.ProtoReceive<StreamAvaliableMessage> ();
			TcpClient streamClient = new TcpClient ();
			streamClient.Connect (IPAddress.Loopback, this.Port);
			streamClient.GetStream ().ProtoSend (new InitMessage (InitAction.Stream));
			streamClient.GetStream ().ProtoSend (avalMsg);
			
			return new ServiceStream (streamClient);
		}
		
		public T GetService<T> () where T : ServiceProxy
		{
			
		}
		
		public void Close ()
		{
		}
		
		public void Dispose ()
		{
			try {
				this.Close ();
			} catch {
			}
		}
	}
}