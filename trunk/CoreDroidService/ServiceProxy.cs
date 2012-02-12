using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using CoreDroid.Extensions;
using CoreDroid.Contract.Message;
using System.IO;
using CoreDroid.Contract;

namespace CoreDroid
{
	public class ServiceProxy
	{
		private SocketClient client;
		private TcpClient tcpClient;

		private NetworkStream stream {
			get{ return this.tcpClient.GetStream ();}
		}
		
		internal void Initialize (SocketClient client, TcpClient tcpClient)
		{
			this.client = client;
			this.tcpClient = tcpClient;
			
			this.onInitialize ();
		}
		
		public virtual void onInitialize ()
		{
		}
		
		public virtual void Close ()
		{
			this.stream.DataSend (new ServiceRequestMessage (ServiceRequestAction.Close));
			this.stream.Close ();
			this.tcpClient.Close ();
		}
		
		protected object Call (string childName, Type returnType, params object[] parameters)
		{
			this.stream.DataSend (new ServiceRequestMessage (ServiceRequestAction.Call));
			
			List<ParameterInfo > parameterInfos = new List<ParameterInfo> ();
			
			foreach (object parameter in parameters) {
				parameterInfos.Add (new ParameterInfo (parameter != null ? parameter.GetType () : null));
			}
			
			this.stream.DataSend (new ServiceCallMessage (childName, parameterInfos.ToArray ()));
			
			foreach (object parameter in parameters) {
				if (parameter != null)
					this.stream.DataSend (parameter);
			}
			
			OperationResultMessage resultMsg = this.stream.DataReceive<OperationResultMessage> ();
			
			if (!resultMsg.Success) {
				throw(new ServiceException (resultMsg));
			}
			
			TypeMessage typeMsg = this.stream.DataReceive<TypeMessage> ();
			
			object retVal = null;
			if (returnType != null && !typeMsg.IsNull) {
				if (returnType.IsSubclassOf (typeof(Stream))) {
					retVal = this.client.GetStream (this.stream);
				} else {
					retVal = this.stream.DataReceive (returnType);
				}
			}
			
			return retVal;
		}
	}
}