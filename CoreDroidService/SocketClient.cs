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
		public int Port { get; private set; }

		public SocketClient (int port)
		{
			this.Port = port;
		}
		
		public void LoadMono (Stream dllStream)
		{
			TcpClient client = new TcpClient ();
			client.Connect (IPAddress.Loopback, this.Port);
			NetworkStream stream = client.GetStream ();
			
			stream.DataSend (new InitMessage (InitAction.LoadMono));
			
			using (ServiceStream destStream = this.GetStream(stream)) {
				dllStream.CopyTo (destStream);
			}
			
			stream.DataSend (new SendingPluginFinishedMessage ());
			OperationResultMessage msg = stream.DataReceive<OperationResultMessage> ();
			
			if (!msg.Success)
				throw(new ServiceException (msg));
		}
		
		internal ServiceStream GetStream (NetworkStream stream)
		{
			StreamAvaliableMessage avalMsg = stream.DataReceive<StreamAvaliableMessage> ();
			TcpClient streamClient = new TcpClient ();
			streamClient.Connect (IPAddress.Loopback, this.Port);
			streamClient.GetStream ().DataSend (new InitMessage (InitAction.Stream));
			streamClient.GetStream ().DataSend (avalMsg);
			
			return new ServiceStream (streamClient);
		}
		
		public T GetService<T> () where T : ServiceProxy
		{
			TcpClient streamClient = new TcpClient ();
			streamClient.Connect (IPAddress.Loopback, this.Port);
			streamClient.GetStream ().DataSend (new InitMessage (InitAction.Start));
			streamClient.GetStream ().DataSend (new TypeMessage (typeof(T).Assembly.GetName ().Name + ".Plugin", typeof(T).FullName));
			T service = Activator.CreateInstance<T> ();
			OperationResultMessage msg = streamClient.GetStream ().DataReceive<OperationResultMessage> ();
			
			if (msg.Success)
				service.Initialize (this, streamClient);
			else
				throw(new ServiceException (msg));
			
			return service;
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