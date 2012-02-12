using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
 
using System.IO;
using System.Runtime.Serialization;

namespace CoreDroid.Extensions
{
	public static class StreamExtensions
	{
		public static object DataReceive (this Stream stream, Type type)
		{
			byte[] lengthBytes = new byte[4];
			for (int i = 0; i < 4; i++)
				lengthBytes [i] = (byte)stream.ReadByte ();
			
			int length = BitConverter.ToInt32 (lengthBytes, 0);
			byte[] buffer = new byte[length];
			stream.Read (buffer, 0, length);
			
			byte[] objectBytes = Convert.FromBase64String (Encoding.ASCII.GetString (buffer));
			
			if (type == typeof(byte[])) {
				return objectBytes;
			} else {
				using (MemoryStream ms = new MemoryStream()) {
					ms.Write (objectBytes, 0, objectBytes.Length);
				
					ms.Position = 0;
				
					return new DataContractSerializer (type).ReadObject (ms);
				}
			}
		}

		public static T DataReceive<T> (this Stream stream)
		{
			return (T)stream.DataReceive (typeof(T));
		}

		public static void DataSend (this Stream stream, object message)
		{
			string base64 = string.Empty;
			if (message is byte[]) {
				base64 = Convert.ToBase64String ((byte[])message);
			} else {
				using (MemoryStream ms = new MemoryStream()) {
					new DataContractSerializer (message.GetType ()).WriteObject (ms, message);
				
					ms.Position = 0;
				
					base64 = Convert.ToBase64String (ms.ToArray ());
				}
			}
			
			byte[] objectBytes = Encoding.ASCII.GetBytes (base64);
			stream.Write (BitConverter.GetBytes (objectBytes.Length), 0, 4);
			stream.Write (objectBytes, 0, objectBytes.Length);
		}
	}
}
