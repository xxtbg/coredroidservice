using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using CoreDroid.Messages;

namespace CoreDroid
{
    public class Service
    {
        private Stream stream;

        internal void Run(Stream stream)
        {
            this.stream = stream;

            SocketService.SendMessage(this.stream, new ServiceInitializedMessage(this.GetType()));

            // TODO LOOP
        }

        protected void SendMessage(object message)
        {
            SocketService.SendMessage(this.stream, message);
        }

        protected void SendStream(Stream stream)
        {

        }

        public abstract void OnReceiveMessage(object message);

        public abstract void Close();
    }
}
