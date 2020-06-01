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

		public BaseServer() {
			

			ConnectedUsers = new List<User>();
			messageQueue = new Queue<UdpReceiveResult>();
		}

		public virtual void BindTo(IPEndPoint endpoint) {
			ListeningEndpoint = endpoint;
			udpClient = new UdpClient(endpoint);
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
			byte[] data = ClassSerializer.Serialize(packet);
			ByteUtil.DumpNum(data);
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
			//int headerSize = Marshal.SizeOf(typeof(T));
			//int sliceSize = Marshal.SizeOf(typeof(U));
			int headerSize = ClassSerializer.GetProfile(typeof(T)).BufferLength;
			int sliceSize = ClassSerializer.GetProfile(typeof(U)).BufferLength;
			byte[] datagram = new byte[headerSize + (sliceSize * slices.Length)];

			byte[] header = ClassSerializer.Serialize(packet);
			ByteUtil.Put(0, header, ref datagram);

			for (int i = 0; i< slices.Length; i++) {
				byte[] slicedata = ClassSerializer.Serialize(slices[i]);
				ByteUtil.Put(headerSize + (i * sliceSize), slicedata, ref datagram);
			}

			ByteUtil.DumpNum(datagram);
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
		public void AddListener<T>(Protocol code, Callback<T> method) where T : IPacket, new() {
			OnUserMessage += (sender, bytedata) => {
				if (bytedata[0] == (byte)code) {
					T payload;

					try {
						payload = ClassSerializer.Deserialize<T>(bytedata);
					} catch(Exception) {
						return;
					}
					method(sender, payload);
				}
			};
		}
		public void AddListener<T, U>(Protocol code, ListCallback<T, U> method)
			where T : IPacketHeader, new()
			where U : IDataSlice, new() {
			OnUserMessage += (sender, bytedata) => {
				if (bytedata[0] == (byte)code) {
					int headerSize = ClassSerializer.GetProfile(typeof(T)).BufferLength;
					int sliceSize = ClassSerializer.GetProfile(typeof(U)).BufferLength;
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
							slice = ClassSerializer.Deserialize<U>(sliceData);
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

		protected virtual (bool accept, string reason) OnConnectingCheck(INetworkPeer peer, CConnectRequest packet) {
			foreach(User user in ConnectedUsers) {
				if (user.Nickname == packet.Nickname) {
					return (false, "Nickname already taken.");
				}
			}

			return (true, "");
		}

		protected virtual void OnUserConnect(User user) {
			ConnectedUsers.Add(user);
		}
		protected virtual void OnUserDisconnect(User user) {
			ConnectedUsers.Remove(user);
		}

		public void ReadNextPacket() {
			var recv = messageQueue.Dequeue();

			ByteUtil.DumpNum(recv.Buffer);

			if (recv.Buffer[0] == (byte)Protocol.C_Connect) {
				
				var peer = new NetworkPeer() { IPAddress = recv.RemoteEndPoint };
				CConnectRequest packet;

				try {
					packet = ClassSerializer.Deserialize<CConnectRequest>(recv.Buffer);
				} catch(Exception e) {
					// packet was mutated, ignore and move on.
					//return;
					throw e;
				}
				
				(bool accept, string reason) = OnConnectingCheck(peer, packet);
				if (accept == true) {
					User user = new User() {
						NetworkID = Guid.NewGuid(),
						Nickname = packet.Nickname,
						IPAddress = peer.IPAddress,
					};
					Send(user, new SPConnectAccept(user.NetworkID));
					OnUserConnect(user);
				} else {
					Send(peer, new SPConnectDeny(reason));
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
			}
		}

		public virtual void Update(float deltaSeconds) {
			for (int i = 0; i < messageQueue.Count; i++) {
				ReadNextPacket();
			}
		}
	}
}
