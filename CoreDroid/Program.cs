using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace CoreDroid
{
	class Program
	{
		static void Main (string[] args)
		{
			int port = 10000;
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties ();
			TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections ();

			foreach (TcpConnectionInformation tcpi in tcpConnInfoArray.OrderBy(i=>i.LocalEndPoint.Port)) {
				if (tcpi.LocalEndPoint.Port == port) {
					port++;
				} else
					break;
			}

			Console.WriteLine (port);

			SocketService.Start (port);
		}
	}
}
