using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace CoreDroid.Client
{
    public class ServiceStream : Stream
    {
        private NetworkStream netStream;
        private Stream srcStream;

        public ServiceStream(NetworkStream netStream, Stream srcStream)
        {
            this.netStream = netStream;
            this.srcStream = srcStream;
        }

        public override bool CanRead
        {
            get { return this.srcStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.srcStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.srcStream.CanWrite; }
        }

        public override void Flush()
        {
            // TODO Send Flush Message
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return this.srcStream.Length; }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // TODO Send Read Message
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // TODO Send Seek Message
            // message.Position == 0 ? SeekOrigin.Begin : (message.Position == 1 ? SeekOrigin.Current : SeekOrigin.End)
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            // TODO Send SetLength Message
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // TODO Send Write Message and Data
            throw new NotImplementedException();
        }

        public override void Close()
        {
            // TODO Send Close Message
            throw new NotImplementedException();
        }
    }
}
