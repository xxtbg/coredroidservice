using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using CoreDroid.Messages.Error;

namespace CoreDroid
{
    public class SocketService
    {
        public static delegate void ReceivedWaiting(Stream s);

        private static Dictionary<int, ReceivedWaiting> waitings = new Dictionary<int, ReceivedWaiting>();

        private static Dictionary<Thread, Service> services = new Dictionary<Thread, Service>();

        private static TypeModel model = TypeModel.Create();

        private static bool run = true;

        public static int RegisterWaiting(ReceivedWaiting callback)
        {
            int newId = 1;

            lock(waitings)
            {
                if(waitings.Count > 0)
                    newId = waitings.Keys.Max() + 1;

                waitings.Add(newId, callback);
            }

            return newId;
        }


        public static void Start(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            while (run)
            {
                while (!listener.Pending()) { Thread.Sleep(200); }
                Socket newSocket = listener.AcceptSocket();

                Thread thread = new Thread(new ParameterizedThreadStart(Handshaker));
                
                lock(services)
                {
                    services.Add(thread, null);
                }

                thread.Start(newSocket);
            }
        }

        private static void Handshaker(object param)
        {
            using (Socket socket = (Socket)param)
            {
                using (NetworkStream stream = new NetworkStream(socket))
                {
                    Type type = GetType(stream);

                    if (!type.IsSubclassOf(typeof(Service)))
                        SendMessage(stream, new WrongTypeErrorMessage(type, "initial type has to be a CoreDroid.Service"));

                    Service service = null;

                    try
                    {
                        service = (Service)Activator.CreateInstance(type);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(stream, new ServiceInitzializationErrorMessage(type, ex));
                    }

                    try
                    {
                        service.Run(stream);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(stream, new ServiceRuntimeErrorMessage(type, ex));
                    }
                }
            }
        }

        internal static Type GetType(Stream stream)
        {
            TypeMessage typeMessage = (TypeMessage)model.Deserialize(stream, null, typeof(TypeMessage));

            if (typeMessage.Assembly == null)
                SendMessage(stream, new AssemblyNotFoundErrorMessage(typeMessage.AssemblyName));

            if (typeMessage.Type == null)
                SendMessage(stream, new TypeNotFoundErrorMessage(typeMessage.AssemblyName, typeMessage.TypeName));
            
            return typeMessage.Type;
        }

        internal static void SendMessage(Stream stream, object message)
        {
            Type type = message.GetType();

            model.Serialize(stream, new TypeMessage(type.Assembly.GetName().Name, type.FullName));
            model.Serialize(stream, message);
        }

        internal static void Stop()
        {
            run = false;

            foreach (KeyValuePair<Thread, Service> service in services)
            {
                service.Value.Close();
                service.Key.Abort();
            }
        }
    }
}
