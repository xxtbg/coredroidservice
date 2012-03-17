using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;
using System.Reflection;

namespace CoreDroid
{
	class MainClass
	{
		static void Main (string[] args)
		{
			int port = 10000;
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties ();
			TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections ();

			foreach (TcpConnectionInformation tcpi in tcpConnInfoArray.OrderBy(i=>i.LocalEndPoint.Port)) {
				if (tcpi.LocalEndPoint.Port == port) {
					port++;
				}
			}

			Console.WriteLine (port);

			SocketService.Start (port);
		}
	}
}
