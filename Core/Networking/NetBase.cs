using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RunGun.Core.Networking
{

	/// <summary>
	/// Minimum interface contract of: Any serializable and deserializable network datagram.
	/// </summary>
	public interface INetStructure { }
	public interface IDataSlice : INetStructure { }
	public interface IPacketHeader : INetStructure {
		
	}
	public interface IPacket : IPacketHeader  { }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
	public abstract class BasePacket : IPacket
	{
		//[FieldOffset(0)] public readonly Protocol Code;

		public BasePacket() {}
	}
	public interface INetworkPeer
	{
		IPEndPoint IPAddress { get; set; }
	}
	public interface IUser : INetworkPeer
	{
		Guid NetworkID { get; set; }

		void Kick();
	}

	public class NetworkPeer : INetworkPeer
	{
		public IPEndPoint IPAddress { get; set; }
	}

	public class User : NetworkPeer, IUser
	{
		public Guid NetworkID { get; set; }
		public string Nickname { get; set; }
		public float KeepAlive { get; set; }

		public void Kick() {
			throw new NotImplementedException();
		}
	}

	public class ConnectRequestEventArgs : EventArgs
	{
		public bool DenyConnection { get; set; }
		public NetworkPeer Sender { get; set; }
		public string DenialReason { get; set; } = "Server denied your request to connect!";
		public byte[] SentData;
	}

	public delegate void C_Callback<T>(T packet);
	public delegate void Callback<T>(User sender, T packet);
	public delegate void ListCallback<T, U>(User sender, T header, List<U> data);

	public delegate void Listener(User sender, byte[] bytedata);
	public delegate void C_Listener(byte[] bytedata);
	public delegate void UserListener(User user);

	public delegate void Bastard(object sender, ConnectRequestEventArgs args);

	public interface IServer
	{
		public float UsersTimeoutAfter { get; set; }
		bool IsListening { get; }
		IPEndPoint ListeningEndpoint { get;}
		//List<User> ConnectedUsers { get; }
		//event Bastard OnUserConnectRequest;
		event Listener OnUserMessage;
		void StartListening();
		void StopListening();
		void AddListener<T>(Protocol code, Callback<T> method) where T :  IPacket;
		void AddListener<T, U>(Protocol code, ListCallback<T, U> method) where T : IPacketHeader where U : IDataSlice;
		void Update(float deltaSeconds);
		void Send<T>(INetworkPeer peer, T packet) where T : IPacket;
		void SendToAll<T>(T packet) where T : IPacket;
		void SendToAllExcept<T>(User user, T packet) where T : IPacket;
		void Send<T, U>(INetworkPeer peer, T header, U[] slices) where T : IPacketHeader where U : IDataSlice;
		void SendToAll<T, U>(T header, U[] slices) where T : IPacketHeader where U : IDataSlice;
		void SendToAllExcept<T, U>(User user, T header, U[] slices) where T : IPacketHeader where U : IDataSlice;
	}
	public interface IClient
	{
		bool IsConnected { get; }
		void Connect(IPEndPoint endpoint, string nickname);
		//void AddListener<T>(byte code, Callback<T> callback) where T: IPacket;
		void Send<T>(T packet) where T : IPacket;
		void Send<T, U>(T packet, U[] slices) where T : IPacketHeader where U : IDataSlice;
	}
}
