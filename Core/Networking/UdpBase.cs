using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RunGun.Core.Networking
{
	public struct Received
	{
		public IPEndPoint Sender;
		public byte[] Packet;
	}

	public abstract class UdpBase
	{
		protected UdpClient Client;

		protected UdpBase() {
			Client = new UdpClient();
		}

		public async Task<Received> Receive() {
			var result = await Client.ReceiveAsync();

			return new Received() {
				Packet = result.Buffer,
				Sender = result.RemoteEndPoint
			};
		}
	}
}
