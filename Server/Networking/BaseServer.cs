using RunGun.Core.Networking;
using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RunGun.Server.Networking
{
	abstract class BaseServer : IServer
	{
		protected UdpClient udpClient;
		private Queue<UdpReceiveResult> messageQueue;
		private bool isThreadRunning = false;
		public float UsersTimeoutAfter { get; set; }
		public bool IsListening { get; }
		public IPEndPoint ListeningEndpoint { get; private set; }
		public List<User> ConnectedUsers;
		public string ServerName { get; set; }

		public event Listener OnUserMessage;

		public BaseServer(IPEndPoint endpoint) {
			ListeningEndpoint = endpoint;

			ConnectedUsers = new List<User>();
			messageQueue = new Queue<UdpReceiveResult>();
			udpClient = new UdpClient(endpoint);
		}

		[Conditional("NOTHING")]
		void DumpDataOut(byte[] data) {
			Console.Write("SO: ");
			for (int i = 0; i < data.Length; i++) {
				Console.Write("{0} ", data[i]);
			}
			Console.WriteLine("");
		}

		[Conditional("NOTHING")]
		void DumpData(byte[] data) {
			Console.Write("SI: ");
			for (int i = 0; i < data.Length; i++) {
				Console.Write("{0} ", data[i]);
			}
			Console.WriteLine("");
		}

		private User GetUserByIP(IPEndPoint endpoint) {
			foreach (User user in ConnectedUsers) {
				if (user.IPAddress.GetHashCode() == endpoint.GetHashCode()) {
					return user;
				}
			}
			return null;
		}

		public void Send<T>(INetworkPeer peer, T packet) where T : IPacket {
			byte[] data = ByteUtil.Serialize(packet);
			DumpDataOut(data);
			try {
				
				udpClient.Send(data, data.Length, peer.IPAddress);
			} catch (SocketException exception) {
				Console.WriteLine("ERR: "+ exception.ToString());
				// Error codes:
				// https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
			}
		}
		public void SendToAll<T>(T packet) where T : IPacket {
			foreach (User user in ConnectedUsers) {
				Send(user, packet);
			}
		}
		public void SendToAllExcept<T>(User exception, T packet) where T : IPacket {
			foreach (User user in ConnectedUsers) {
				if (!user.Equals(exception)) {
					Send(user, packet);
				}
			}
		}
		public void Send<T, U>(INetworkPeer peer, T packet, U[] slices) where T : IPacketHeader where U : IDataSlice {
			int headerSize = Marshal.SizeOf(typeof(T));
			int sliceSize = Marshal.SizeOf(typeof(U));
			byte[] datagram = new byte[headerSize + (sliceSize * slices.Length)];

			byte[] header = ByteUtil.Serialize(packet);
			ByteUtil.Put(0, header, ref datagram);

			for (int i = 0; i< slices.Length; i++) {
				byte[] slicedata = ByteUtil.Serialize(slices[i]);
				ByteUtil.Put(headerSize + (i * sliceSize), slicedata, ref datagram);
			}
			DumpDataOut(datagram);
			try {
				udpClient.Send(datagram, datagram.Length, peer.IPAddress);
			} catch (SocketException exception) {
				Console.WriteLine("Exception:" + exception.ToString());

				
				// Error codes:
				// https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
			}
		}
		public void SendToAll<T, U>(T packet, U[] slices) where T : IPacketHeader where U : IDataSlice {
			foreach (User user in ConnectedUsers) {
				// TODO: can optimize
				Send(user, packet, slices);
			}
		}
		public void SendToAllExcept<T, U>(User exception, T packet, U[] slices) where T : IPacketHeader where U : IDataSlice {
			foreach (User user in ConnectedUsers) {
				if (!user.Equals(exception)) {
					Send(user, packet, slices);
				}
			}
		}
		public void AddListener<T>(Protocol code, Callback<T> method) where T : IPacket {
			OnUserMessage += (sender, bytedata) => {
				if (bytedata[0] == (byte)code) {
					T payload;

					try {
						payload = ByteUtil.Deserialize<T>(bytedata);
					} catch(Exception) {
						return;
					}
					method(sender, payload);
				}
			};
		}
		public void AddListener<T, U>(Protocol code, ListCallback<T, U> method)
			where T : IPacketHeader
			where U : IDataSlice {
			OnUserMessage += (sender, bytedata) => {
				if (bytedata[0] == (byte)code) {
					int headerSize = Marshal.SizeOf(typeof(T));
					int sliceSize = Marshal.SizeOf(typeof(U));
					int numSlices = (bytedata.Length - headerSize) / sliceSize;
					byte[] headerData = new byte[headerSize];
					Array.Copy(bytedata, 0, headerData, 0, headerSize);
					T header;
					
					try {
						header = ByteUtil.Deserialize<T>(headerData);
					} catch (Exception) {
						return;
					}
					
					List<U> slices = new List<U>();

					for (int i = 0; i < numSlices; i++) {
						byte[] sliceData = new byte[sliceSize];
						Array.Copy(bytedata, headerSize + (sliceSize * i), sliceData, 0, sliceSize);

						U slice;

						try {
							slice = ByteUtil.Deserialize<U>(sliceData);
						} catch(Exception) {
							return;
						}

						slices.Add(slice);
					}
					method(sender, header, slices);
				}
			};
		}

		private async void NetworkThread() {
			while (isThreadRunning) {
				try {
					UdpReceiveResult rec = await udpClient.ReceiveAsync();
					lock (messageQueue) {
						messageQueue.Enqueue(rec);
					}
				} catch (SocketException e) {
					// Some socket exceptions must be ignored 
					if (e.ErrorCode == 10054) {
						// 10054 @ An existing connection was forcibly closed by the remote host.
					} else {
						//Console.WriteLine(String.Format(
						//"SocketException: {0} @ {1}", e.ErrorCode, e.Message));
						throw e;
					}
				}
			}
		}

		public void StartListening() {
			isThreadRunning = true;
			Task.Factory.StartNew(NetworkThread);
		}
		public void StopListening() {
			isThreadRunning = false;
		}

		protected virtual (bool accept, string reason) OnConnectingCheck(INetworkPeer peer, C_ConnectRequestPacket packet) {
			foreach(User user in ConnectedUsers) {
				if (user.Nickname == packet.Nickname) {
					return (false, "Nickname already taken.");
				}
			}

			return (true, "");
		}

		protected virtual void OnUserConnect(User user) {
			//Logging.Out(user.Nickname + " connected");
			ConnectedUsers.Add(user);
		}

		protected virtual void OnUserDisconnect(User user) {
			//Logging.Out(user.Nickname + " disconnected");
			ConnectedUsers.Remove(user);
		}

		public void ReadNextPacket() {
			var recv = messageQueue.Dequeue();

			DumpData(recv.Buffer);

			if (recv.Buffer[0] == (byte)Protocol.C_Connect) {
				var peer = new NetworkPeer() { IPAddress = recv.RemoteEndPoint };
				C_ConnectRequestPacket packet;

				try {
					packet = ByteUtil.Deserialize<C_ConnectRequestPacket>(recv.Buffer);
				} catch(Exception e) {
					// packet was mutated, ignore and move on.
					return;
				}
				(bool accept, string reason) = OnConnectingCheck(peer, packet);
				if (accept == true) {
					User user = new User();
					user.NetworkID = Guid.NewGuid();
					user.Nickname = packet.Nickname;
					user.IPAddress = peer.IPAddress;
					Send(user, new S_ConnectAcceptPacket(user.NetworkID));
					OnUserConnect(user);
				} else {
					Send(peer, new S_ConnectDenyPacket(reason));
				}

				//OnUserConnectRequest?.Invoke(this, args);
			} else if (recv.Buffer[0] == (byte)Protocol.C_Disconnect) {

				User user = GetUserByIP(recv.RemoteEndPoint);

				if (user != null) {
					OnUserDisconnect(user);
					return;
				}
			} else {
				User user = GetUserByIP(recv.RemoteEndPoint);

				if (user != null) {
					OnUserMessage?.Invoke(user, recv.Buffer);
					return;
				}
				Console.WriteLine("Could not find user?");
				//OnUserMessage?.Invoke(new NetworkPeer() { IPAddress = recv.RemoteEndPoint}, recv.Buffer);
				//return;
			}
		}

		public virtual void Update(float deltaSeconds) {
			for (int i = 0; i < messageQueue.Count; i++) {
				ReadNextPacket();
			}
		}
	}
}
