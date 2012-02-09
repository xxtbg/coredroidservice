using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

using CoreDroid.Extensions;
using CoreDroid.Contract.Message;

namespace CoreDroid
{
    public class ServiceStream : Stream, IDisposable
    {
        private NetworkStream netStream;

        public ServiceStream(NetworkStream netStream)
        {
            this.netStream = netStream;
        }

        public override bool CanRead
        {
            get { return this.DoStreamAction<bool>(StreamAction.CanRead); }
        }

        public override bool CanSeek
        {
            get { return this.DoStreamAction<bool>(StreamAction.CanSeek); }
        }

        public override bool CanWrite
        {
            get { return this.DoStreamAction<bool>(StreamAction.CanWrite); }
        }

        public override void Flush()
        {
            this.DoStreamAction(StreamAction.Flush);
        }

        public override long Length
        {
            get { return this.DoStreamAction<long>(StreamAction.Length); }
        }

        public override long Position
        {
            get
            {
                return this.DoStreamAction<long>(StreamAction.GetPosition);
            }
            set
            {
                this.DoStreamAction(StreamAction.SetPosition, null, value, 0, 0);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = this.DoStreamAction<int>(StreamAction.Read, null, 0, 0, count);
            this.netStream.Read(buffer, offset, read);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // message.Position == 0 ? SeekOrigin.Begin : (message.Position == 1 ? SeekOrigin.Current : SeekOrigin.End)
            return this.DoStreamAction<long>(StreamAction.Seek, null, (origin == SeekOrigin.Begin ? 0 : (origin == SeekOrigin.Current ? 1 : 2)), offset, 0);
        }

        public override void SetLength(long value)
        {
            this.DoStreamAction(StreamAction.SetLength, null, 0, 0, value);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            this.DoStreamAction(StreamAction.Write, buffer, 0, offset, size);
        }

        public override void Close()
        {
            this.DoStreamAction(StreamAction.Close);
        }

        public void Dispose()
        {
            if(this.netStream.CanWrite)
                this.Close();
        }

        private void DoStreamAction(StreamAction action)
        {
            this.DoStreamAction(action, null, 0, 0, 0);
        }

        private T DoStreamAction<T>(StreamAction action)
        {
            return this.DoStreamAction<T>(action, null, 0, 0, 0);
        }

        private void DoStreamAction(StreamAction action, byte[] buffer, long position, long offset, long size)
        {
            this.DoStreamAction<object>(action, buffer, position, offset, size);
        }

        private T DoStreamAction<T>(StreamAction action, byte[] buffer, long position, long offset, long size)
        {
            this.netStream.ProtoSend(new StreamActionMessage(action, position, action != StreamAction.Write ? offset : 0, size));

            if (action == StreamAction.Write)
                this.netStream.Write(buffer, Convert.ToInt32(offset), Convert.ToInt32(size));

            OperationFinishedMessage msg = this.netStream.ProtoReceive<OperationFinishedMessage>();

            if (!msg.Success)
                throw(new ServiceException(msg));

            return this.netStream.ProtoReceive<T>();
        }
    }

    public class ServiceException : Exception
    {
        public string OriginalAssemblyName { get; private set; }
        public string OriginalTypeName { get; private set; }

        public string OriginalMessage { get; private set; }

        public string OriginalStackTrace { get; private set; }

        public ServiceException(OperationFinishedMessage msg)
            : base()
        {
            this.OriginalAssemblyName = msg.ExceptionAssemblyName;
            this.OriginalTypeName = msg.ExceptionTypeName;
            this.OriginalMessage = msg.ExceptionMessage;
            this.OriginalStackTrace = msg.ExceptionStackTrace;
        }
    }
}
