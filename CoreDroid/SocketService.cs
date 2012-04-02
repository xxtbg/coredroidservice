using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
 
using CoreDroid.Extensions;
using CoreDroid.Contract.Message;
using CoreDroid.Contract;

namespace CoreDroid
{
	public static class SocketService
	{
		public delegate void ReceivedWaiting (Stream s);

		private static Dictionary<int, ReceivedWaiting> waitings = new Dictionary<int, ReceivedWaiting> ();
		private static Dictionary<int, Thread> serviceThreads = new Dictionary<int, Thread> ();
		private static Dictionary<int, Stream> waitingStreams = new Dictionary<int, Stream> ();
		private static bool run = true;

		public static int RegisterWaiting (ReceivedWaiting callback)
		{
			int newId = 1;

			lock (waitings) {
				if (waitings.Count > 0)
					newId = waitings.Count > 0 ? waitings.Keys.Max () + 1 : 0;

				waitings.Add (newId, callback);
			}

			return newId;
		}

		public static void Start (int port)
		{
			TcpListener listener = new TcpListener (IPAddress.Loopback, port);
			listener.Start ();

			while (run) {
				while (!listener.Pending() && run) {
					Thread.Sleep (200);
				}
				
				if (!run)
					break;
				
				Socket newSocket = listener.AcceptSocket ();

				Thread thread = new Thread (new ParameterizedThreadStart (Handshaker));

				int id = 0;
				lock (serviceThreads) {
					id = serviceThreads.Count > 0 ? serviceThreads.Keys.Max () + 1 : 0;
					serviceThreads.Add (id, thread);
				}

				thread.Start (new object[] { id, newSocket });
			}
		}

		private static void Handshaker (object param)
		{
			int id = (int)((object[])param) [0];
			Socket socket = (Socket)((object[])param) [1];

			using (socket) {
				using (NetworkStream stream = new NetworkStream(socket)) {
					InitMessage initMsg = stream.DataReceive<InitMessage> ();
					switch (initMsg.Action) {
					case InitAction.LoadMono:
						if (initMsg.Parameter != null) {
							LoadMono (stream, initMsg.Parameter as string);
						} else {
							LoadMono (stream);
						}
						
						break;
					case InitAction.Start:
						TypeInfo msg = stream.DataReceive<TypeInfo> ();
						if (msg.Type != null)
							ServiceStarter (id, stream, msg.Type);
						else
							stream.DataSend (new OperationResultMessage (
								new ArgumentException (string.Concat ("could not find requested service <", msg.TypeName, "> in assembly <", msg.AssemblyName, ">"))));
						break;
					case InitAction.Stream:
						Stream (id, stream, stream.DataReceive<StreamAvaliableMessage> ().Id);
						break;
					case InitAction.Close:
						run = false;
						break;
					}
				}
			}
		}
		
		private static void LoadMono (NetworkStream stream, string assemblyString)
		{
			try {
				AppDomain.CurrentDomain.Load (assemblyString);

				stream.DataSend (new OperationResultMessage (true));
			} catch (Exception ex) {
				stream.DataSend (new OperationResultMessage (ex));
			}
		}

		private static void LoadMono (NetworkStream stream)
		{
			try {
				using (MemoryStream ms = new MemoryStream()) {
					SendStream (stream, ms);
					stream.DataReceive<SendingPluginFinishedMessage> ();

					AppDomain.CurrentDomain.Load (ms.ToArray ());

					stream.DataSend (new OperationResultMessage (true));
				}
			} catch (Exception ex) {
				stream.DataSend (new OperationResultMessage (ex));
			}
		}

		private static void Stream (int id, NetworkStream stream, int streamId)
		{
			Stream streamToSend = waitingStreams [streamId];

			bool closed = false;
			while (!closed) {
				StreamActionMessage message = stream.DataReceive<StreamActionMessage> ();
				
				try {
					switch (message.Action) {
					case StreamAction.CanRead:
						bool canRead = streamToSend.CanRead;
						stream.ActionFinishedSuccess ();
						stream.DataSend (canRead);
						break;
					case StreamAction.CanSeek:
						bool canSeek = streamToSend.CanSeek;
						stream.ActionFinishedSuccess ();
						stream.DataSend (canSeek);
						break;
					case StreamAction.CanWrite:
						bool canWrite = streamToSend.CanWrite;
						stream.ActionFinishedSuccess ();
						stream.DataSend (canWrite);
						break;
					case StreamAction.Length:
						long length = streamToSend.Length;
						stream.ActionFinishedSuccess ();
						stream.DataSend (length);
						break;
					case StreamAction.GetPosition:
						long position = streamToSend.Position;
						stream.ActionFinishedSuccess ();
						stream.DataSend (position);
						break;
					case StreamAction.SetPosition:
						streamToSend.Position = message.Position;
						stream.ActionFinishedSuccess ();
						break;
					case StreamAction.Close:
						closed = true;
						streamToSend.Close ();
						stream.DataSend (new OperationResultMessage (true));
						break;
					case StreamAction.Flush:
						streamToSend.Flush ();
						stream.DataSend (new OperationResultMessage (true));
						break;
					case StreamAction.Read:
						byte[] buffer = new byte[Convert.ToInt32 (message.Size)];
						streamToSend.Read (buffer, 0, Convert.ToInt32 (message.Size));
						stream.DataSend (buffer.Take (Convert.ToInt32 (message.Size)).ToArray ());
						stream.DataSend (new OperationResultMessage (true));
						break;
					case StreamAction.Seek:
						long seeked = streamToSend.Seek (message.Offset, message.Position == 0 ? SeekOrigin.Begin : (message.Position == 1 ? SeekOrigin.Current : SeekOrigin.End));
						stream.DataSend (new OperationResultMessage (true));
						stream.DataSend (seeked);
						break;
					case StreamAction.SetLength:
						streamToSend.SetLength (message.Size);
						stream.DataSend (new OperationResultMessage (true));
						break;
					case StreamAction.Write:
						byte[] data = stream.DataReceive<byte[]> ();
						streamToSend.Write (data, 0, data.Length);
						stream.DataSend (new OperationResultMessage (true));
						break;
					default:
						throw (new NotImplementedException ());
					}
				} catch (Exception ex) {
					stream.DataSend (new OperationResultMessage (ex));
				}
			}

			lock (waitingStreams) {
				waitingStreams.Remove (id);
			}

			ServiceStop (id, null);
		}

		private static void ActionFinishedSuccess (this NetworkStream stream)
		{
			stream.DataSend (new OperationResultMessage (true));
		}

		private static void SendStream (NetworkStream stream, Stream streamToSend)
		{
			int id = 0;

			lock (waitingStreams) {
				id = waitingStreams.Count > 0 ? waitingStreams.Keys.Max () + 1 : 0;
				waitingStreams.Add (id, streamToSend);
			}

			stream.DataSend (new StreamAvaliableMessage (id));
		}

		private static void ServiceStarter (int id, NetworkStream stream, Type type)
		{
			object service = null;
			try {
				if (type.GetCustomAttributes (true).Where (a => a is ServiceContractAttribute).Any ()) {
					service = Activator.CreateInstance (type);
				} else {
					throw (new ArgumentException ("service type has to be a DataContract"));
				}
			} catch (Exception ex) {
				stream.DataSend (new OperationResultMessage (ex));
			}

			if (service != null) {
				try {
					ServiceLooper (stream, service);
				} catch (Exception ex) {
					stream.DataSend (new OperationResultMessage (ex));
				} finally {
					ServiceStop (id, service);
				}
			}
		}

		private static void ServiceLooper (NetworkStream stream, object service)
		{
			bool stopService = false;
			
			stream.DataSend (new OperationResultMessage (true));
			
			while (!stopService) {
				if (stream.DataAvailable) {
					try {
						switch (stream.DataReceive<ServiceRequestMessage> ().Action) {
						case ServiceRequestAction.Call:
							ServiceHandleCall (stream, service);
							break;
						case ServiceRequestAction.Close:
							stopService = true;
							break;
						}
					} catch (Exception ex) {
						try {
							do {
								Thread.Sleep (200);
								stream.Flush ();
							} while (stream.DataAvailable);

							stream.DataSend (new OperationResultMessage (ex));
						} catch {
							stopService = true;
						}
					}
				} else {
					Thread.Sleep (200);
				}
			}
		}

		private static void ServiceHandleCall (NetworkStream stream, object service)
		{
			ServiceCallMessage msg = stream.DataReceive<ServiceCallMessage> ();
			string methodName = msg.ChildName;
			TypeInfo[] parameterInfos = msg.Parameter;

			List<object > parameters = new List<object> ();
			
			foreach (TypeInfo parameterInfo in parameterInfos) {
				if (!parameterInfo.IsNull)
					parameters.Add (stream.DataReceive (parameterInfo.Type));
				else
					parameters.Add (null);
			}
			
			System.Reflection.MethodInfo methodInfo = service.GetType ().GetMethod (methodName);
			if (methodInfo.GetCustomAttributes (true).Where (a => a is ServiceMemberAttribute).Any ()) {
				try {
					object returnValue = methodInfo.Invoke (service, parameters.ToArray ());
				
					if (methodInfo.ReturnType != null) {
						stream.DataSend (new OperationResultMessage (true));
						if (returnValue != null) {
							stream.DataSend (new TypeInfo (returnValue.GetType ()));
						} else {
							stream.DataSend (new TypeInfo (null));
						}
					
						if (returnValue != null) {
							if (returnValue is Stream) {
								SendStream (stream, (Stream)returnValue);
							} else {
								stream.DataSend (returnValue);
							}
						}
					}
				} catch (System.Reflection.TargetInvocationException ex) {
					throw(ex.InnerException);
				}
			} else {
				throw (new ArgumentException ("method is not existing"));
			}
		}

		private static void ServiceStop (int id, object service)
		{
			if (service is IDisposable) {
				((IDisposable)service).Dispose ();
			}

			lock (serviceThreads) {
				serviceThreads.Remove (id);
			}
		}
	}
}
