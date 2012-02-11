using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using ProtoBuf;
using CoreDroid.Extensions;
using System.Reflection;
using CoreDroid.Contract.Message;

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
					newId = waitings.Keys.Max () + 1;

				waitings.Add (newId, callback);
			}

			return newId;
		}

		public static void Start (int port)
		{
			TcpListener listener = new TcpListener (IPAddress.Loopback, port);
			listener.Start ();

			while (run) {
				while (!listener.Pending()) {
					Thread.Sleep (200);
				}
				Socket newSocket = listener.AcceptSocket ();

				Thread thread = new Thread (new ParameterizedThreadStart (Handshaker));

				int id = 0;
				lock (serviceThreads) {
					id = serviceThreads.Keys.Max () + 1;
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
					switch (stream.ProtoReceive<InitMessage> ().Action) {
					case InitAction.LoadMono:
						LoadMono (stream);
						break;
					case InitAction.Start:
						ServiceStarter (id, stream, stream.ProtoReceive<TypeMessage> ().Type);
						break;
					case InitAction.Stream:
						Stream (id, stream, stream.ProtoReceive<StreamAvaliableMessage> ().Id);
						break;
					}
				}
			}
		}

		private static void LoadMono (NetworkStream stream)
		{
			try {
				using (MemoryStream ms = new MemoryStream()) {
					SendStream (stream, ms);
					stream.ProtoReceive<SendingPluginFinishedMessage> ();

					AppDomain.CurrentDomain.Load (ms.ToArray ());

					stream.ProtoSend (new OperationFinishedMessage ());
				}
			} catch (Exception ex) {
				stream.ProtoSend (new OperationFinishedMessage (ex));
			}
		}

		private static void Stream (int id, NetworkStream stream, int streamId)
		{
			Stream streamToSend = waitingStreams [streamId];

			bool closed = false;
			while (!closed) {
				StreamActionMessage message = stream.ProtoReceive<StreamActionMessage> ();
				
				try {
					switch (message.Action) {
					case StreamAction.CanRead:
						bool canRead = streamToSend.CanRead;
						stream.ActionFinishedSuccess ();
						stream.ProtoSend (canRead);
						break;
					case StreamAction.CanSeek:
						bool canSeek = streamToSend.CanSeek;
						stream.ActionFinishedSuccess ();
						stream.ProtoSend (canSeek);
						break;
					case StreamAction.CanWrite:
						bool canWrite = streamToSend.CanWrite;
						stream.ActionFinishedSuccess ();
						stream.ProtoSend (canWrite);
						break;
					case StreamAction.Length:
						long length = streamToSend.Length;
						stream.ActionFinishedSuccess ();
						stream.ProtoSend (length);
						break;
					case StreamAction.GetPosition:
						long position = streamToSend.Position;
						stream.ActionFinishedSuccess ();
						stream.ProtoSend (position);
						break;
					case StreamAction.SetPosition:
						streamToSend.Position = message.Position;
						stream.ActionFinishedSuccess ();
						break;
					case StreamAction.Close:
						closed = true;
						streamToSend.Close ();
						stream.ProtoSend (new OperationFinishedMessage ());
						break;
					case StreamAction.Flush:
						streamToSend.Flush ();
						stream.ProtoSend (new OperationFinishedMessage ());
						break;
					case StreamAction.Read:
						byte[] buffer = new byte[Convert.ToInt32 (message.Size)];
						int read = streamToSend.Read (buffer, 0, Convert.ToInt32 (message.Size));
						stream.ProtoSend (new OperationFinishedMessage ());
						stream.ProtoSend (read);
						stream.Write (buffer, 0, read);
						break;
					case StreamAction.Seek:
						long seeked = streamToSend.Seek (message.Offset, message.Position == 0 ? SeekOrigin.Begin : (message.Position == 1 ? SeekOrigin.Current : SeekOrigin.End));
						stream.ProtoSend (new OperationFinishedMessage ());
						stream.ProtoSend (seeked);
						break;
					case StreamAction.SetLength:
						streamToSend.SetLength (message.Size);
						stream.ProtoSend (new OperationFinishedMessage ());
						break;
					case StreamAction.Write:
						stream.CopyTo (streamToSend, Convert.ToInt32 (message.Size));
						stream.ProtoSend (new OperationFinishedMessage ());
						break;
					default:
						throw (new NotImplementedException ());
					}
				} catch (Exception ex) {
					stream.ProtoSend (new OperationFinishedMessage (ex));
				}
			}

			lock (waitingStreams) {
				waitingStreams.Remove (id);
			}

			ServiceStop (id, null);
		}

		private static void ActionFinishedSuccess (this NetworkStream stream)
		{
			stream.ProtoSend (new OperationFinishedMessage ());
		}

		private static void SendStream (NetworkStream stream, Stream streamToSend)
		{
			int id = 0;

			lock (waitingStreams) {
				id = waitingStreams.Keys.Max () + 1;
				waitingStreams.Add (waitingStreams.Keys.Max () + 1, streamToSend);
			}

			stream.ProtoSend (new StreamAvaliableMessage (id));
		}

		private static void ServiceStarter (int id, NetworkStream stream, Type type)
		{
			object service = null;
			try {
				if (type.GetCustomAttributes (true).Where (a => a is ProtoContractAttribute).Any ()) {
					service = Activator.CreateInstance (type);
					stream.ProtoSend (new ServiceInitializedMessage (type));
				} else {
					throw (new ArgumentException ("service type has to be a ProtoContract"));
				}
			} catch (Exception ex) {
				stream.ProtoSend (new ServiceInitzializationErrorMessage (type, ex));
			}

			if (service != null) {
				try {
					ServiceLooper (stream, service);
				} catch (Exception ex) {
					stream.ProtoSend (new ServiceRuntimeErrorMessage (type, ex));
				} finally {
					ServiceStop (id, service);
				}
			}
		}

		private static void ServiceLooper (NetworkStream stream, object service)
		{
			bool stopService = false;

			while (!stopService) {
				if (stream.DataAvailable) {
					try {
						switch (stream.ProtoReceive<RequestMessage> ().Action) {
						case RequestAction.Call:
							ServiceHandleCall (stream, service);
							break;
						case RequestAction.Close:
							stopService = true;
							break;
						}
					} catch (Exception ex) {
						try {
							do {
								Thread.Sleep (200);
								stream.Flush ();
							} while (stream.DataAvailable);

							stream.ProtoSend (new ServiceRuntimeErrorMessage (service.GetType (), ex));
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
			ServiceCallMessage msg = stream.ProtoReceive<ServiceCallMessage> ();
			string methodName = msg.MethodName;
			MethodParameterInfo[] methodParameterInfos = msg.MethodParameterInfos;

			Dictionary<string, object > methodParameters = new Dictionary<string, object> ();

			foreach (MethodParameterInfo methodParameterInfo in methodParameterInfos) {
				methodParameters.Add (methodParameterInfo.Name, stream.ProtoReceive (methodParameterInfo.Type));
			}

			MethodInfo methodInfo = service.GetType ().GetMethod (methodName);

			if (methodInfo.GetCustomAttributes (true).Where (a => a is ProtoMemberAttribute).Any ()) {
				object returnValue = methodInfo.Invoke (service, methodParameters.Values.ToArray ());

				if (methodInfo.ReturnType != null) {
					stream.ProtoSend (new ServiceReturnMessage (returnValue != null ? returnValue.GetType () : null));

					if (returnValue != null) {
						if (returnValue is Stream) {
							SendStream (stream, (Stream)returnValue);
						} else {
							stream.ProtoSend (returnValue);
						}
					}
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
