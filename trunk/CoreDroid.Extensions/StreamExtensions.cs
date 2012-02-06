using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ProtoBuf;
using ProtoBuf.Meta;
using System.IO;

namespace CoreDroid.Extensions
{
    public static class StreamExtensions
    {
        private static TypeModel model = TypeModel.Create();

        public static object ProtoReceive(this Stream stream, Type type)
        {
            return model.Deserialize(stream, null, type);
        }

        public static T ProtoReceive<T>(this Stream stream)
        {
            return (T)stream.ProtoReceive(typeof(T));
        }

        public static void ProtoSend(this Stream stream, object message)
        {
            model.Serialize(stream, message);
        }
    }
}
