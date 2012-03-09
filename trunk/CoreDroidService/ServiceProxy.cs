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
		
		protected object Call (string childName, params object[] parameters)
		{
			this.stream.DataSend (new ServiceRequestMessage (ServiceRequestAction.Call));
			
			List<TypeInfo > parameterInfos = new List<TypeInfo> ();
			
			foreach (object parameter in parameters) {
				parameterInfos.Add (new TypeInfo (parameter != null ? parameter.GetType () : null));
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
			
			TypeInfo typeMsg = this.stream.DataReceive<TypeInfo> ();
			
			object retVal = null;
			if (typeMsg.Type != null && !typeMsg.IsNull) {
				if (typeMsg.Type.IsSubclassOf (typeof(Stream))) {
					retVal = this.client.GetStream (this.stream);
				} else {
					Type type = typeMsg.Type;
					if (typeMsg.IsArrayType) {
						type = type.MakeArrayType ();
					} else if (typeMsg.GenericArguments != null) {	
						type = type.MakeGenericType (typeMsg.GenericArguments.Select (ti => ti.Type).ToArray ());
					}
					
					retVal = this.stream.DataReceive (type);
				}
			}
			
			return retVal;
		}
	}
}