using System.Net.Sockets;
using System.Net;

namespace Blocks.Core;

public static class Network
{
		public static string GetLocalIPAddress()
		{
				var host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (var ip in host.AddressList)
				{
						if (ip.AddressFamily == AddressFamily.InterNetwork)
						{
								return ip.ToString();
						}
				}
				throw new Exception("No network adapters with an IPv4 address in the system!");
		}
}
