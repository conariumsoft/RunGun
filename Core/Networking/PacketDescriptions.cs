/* RunGun Source Code
 * Copyright Conarium Softare 2019-2020 (All Rights Reserved)
 * Class: RunGun.Core.Networking.PacketDescriptions.cs
 * Description: Defines structure of networking packets.
 * Maintainer: Joshuu O'Leary
 * Revision: 1.0 (April 1, 2020)
 * To-Do List:
 *
 *
 * Notes:
 *
 *
 */
using prototypecode;
using RunGun.Core.Game;
using RunGun.Core.Networking;
using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using static System.Net.IPAddress;
// TODO: make network protocol little-endian
// ur gonna have to redesign everything LUL
namespace prototypecode
{
	// use the protocol and schema attributes to generate a perfect-fit serializer 
	// and deserializer for each datagram type (at runtime, meaning I don't have to manually change them. 
	// they will auto-update to whatever I make the packets)
	// and then i'll cache the resulting serialization methods after generating the first one, 
	// so I don't slow anything down

	public class ByteBuffer
	{
		private int bufferSize;
		private int spaceFilled;

		private byte[] buffer;

		public ByteBuffer(int size) {
			bufferSize = size;
			spaceFilled = 0;
			buffer = new byte[bufferSize];
		}

		public void Push(params byte[] bytes) {
			if (bytes.Length > (bufferSize - spaceFilled))
				throw new Exception("Attempt to overfill a byte buffer");

			for (int i = 0; i < bytes.Length; i++) {
				buffer[spaceFilled] = bytes[i];
				spaceFilled++;
				Console.WriteLine(spaceFilled);
			}
		}

		public byte Pull() {
			spaceFilled--;
			byte data = buffer[spaceFilled];

			return data;
		}
		public void Pull(int quantity, ref byte[] buff) {
			for (int i = quantity-1; i >= 0; i--) {
				spaceFilled--;
				buff[i] = buffer[spaceFilled];
			}
		}
		public byte[] Copy() {
			return buffer;
		}
		public void Copy(ref byte[] destination) {
			Array.Copy(buffer, 0, destination, 0, buffer.Length);
		}
		public void Copy(int start, int length, ref byte[] destination, int destinationIndex = 0) {
			Array.Copy(buffer, start, destination, destinationIndex, length);
		}
		public void Clear() {
			for (int i = 0; i < bufferSize; i++) {
				buffer[i] = default;
			}
		}
		public byte Peek(int index) {
			return buffer[index];
		}
	}

	public class SmartBuffer
	{
		public void Push<T>(T instance) {

		}
	}

	public struct MemberSerializationProfile
	{
		public Type MemberType { get; set; }
		public string MemberName { get; set; }
		public int MemberBufferIndex { get; set; }
	}
	
	public static class TypeSer
	{
		public static T To<T>(byte[] data) {


			return default(T);
		}
		public static byte[] From(object obj) {

			Type t = obj.GetType();

			if (t == typeof(short))  return FromShort((short)obj);
			if (t == typeof(int))    return FromInt((int)obj);
			if (t == typeof(float))  return FromFloat((int)obj);
			if (t == typeof(double)) return FromDouble((double)obj);

			

			throw new Exception();

		}
		public static byte[] FromShort(short input) {
			return BitConverter.GetBytes(HostToNetworkOrder(input));
		}
		public static byte[] FromUShort(ushort input) { }		
		public static byte[] FromInt(int input) {
			return BitConverter.GetBytes(HostToNetworkOrder(input));
		}

		
		public static void FromFloat()

		public static byte[] ToUShort(ushort input) {
			return null;
		}
		public static short ToShort(byte[] input) {
			return NetworkToHostOrder(BitConverter.ToInt16(input, 0));
		}
		public static byte[] ToUInt(uint input) {
			return null;
		}
		public static byte[] ToFloat(int input) {
			return null;
		}
		public static byte[] ToDouble(int input) {
			return null;
		}
		public static byte[] ToGuid(int input) {
			return null;
		}
		public static byte[] ToString(string input, int length) {
			return null;
		}

		public static void Short(short input, ref byte[] output) { }
		public static void UShort(ushort input, ref byte[] output) { }
		public static void Int(int input, ref byte[] output) { }
	}

	public class SerializationProfile
	{
		public List<MemberSerializationProfile> Fields;
		public byte[] Buffer;
	}

	/* System for tagging objects as packets
	 * 
	 * Parse the Attribute data, figure out what fields in the packet
	 * 
	 * Create a lookup table of fields, so that reflection 
	 * lookup doesn't have to be done after the first time
	 * 
	 */

	public static class Balls
	{
		static Dictionary<Type, ByteBuffer> buffers = new Dictionary<Type, ByteBuffer>();

		static Dictionary<Type, SerializationProfile> profiles = new Dictionary<Type, SerializationProfile>();

		public static void GenerateProfile(Type type) {

			SerializationProfile profile = new SerializationProfile();
			PropertyInfo[] properties = type.GetProperties();

			int bufferOffset = 0;
			for (int i = 0; i < properties.Length; i++) {
				PropertyInfo prop = properties[i];

				Schema schema = (Schema)Attribute.GetCustomAttribute(prop, typeof(Schema));
				if (schema == null)
					continue;

				Type propType = prop.PropertyType;

				profile.Fields.Add(new MemberSerializationProfile {
					MemberType = prop.PropertyType,
					MemberName = prop.Name,
					MemberBufferIndex = bufferOffset
				});

				if (propType == typeof(short)) { bufferOffset += 2; }
				if (propType == typeof(int)) { bufferOffset += 4; }
				if (propType == typeof(float)) { bufferOffset += 4; }
			}

			profiles.Add(type, profile);
		}

		public static void Serialize<T>(T inst) {
			Type type = inst.GetType();

			bool Le = BitConverter.IsLittleEndian;

			if (profiles.ContainsKey(type)) {
				SerializationProfile profile = profiles[type];

				// clear profile buffer here
				ByteBuffer buffer = buffers[type];
				buffer.Clear();
				for (int i = 0; i < profile.Fields.Count; i++) {
					MemberSerializationProfile member = profile.Fields[i];
					Type memberType = member.MemberType;

					PropertyInfo info = inst.GetType().GetProperty(member.MemberName);

					object retrieved = info.GetValue(inst, null);

					buffer.Push(TypeSer.From(retrieved));
				}

			} else {
				GenerateProfile(type);
			}
		}
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	class Packet : Attribute
	{

	}

	class Schema : Attribute
	{

	}

	[Packet]
	class PotentialPacket
	{
		[Schema] int fi;
		[Schema] int fo { get; set; } = 2;
		[Schema] int umom { get; set; }
	}

	//public interface fuckhead
	//{
		//public static ByteBuffer Buffer { get; }
	//}

}

namespace RunGun.Core.Networking
{

	#region Server Packets
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
	class S_ConnectAcceptPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.S_ConnectOK;
		public Guid UserGUID;

		public S_ConnectAcceptPacket() { }
		public S_ConnectAcceptPacket(Guid id) {
			UserGUID = id;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	class S_ConnectDenyPacket : BasePacket
	{

		public readonly Protocol Code = Protocol.S_ConnectDeny;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string DenyReason;

		public S_ConnectDenyPacket() { }
		public S_ConnectDenyPacket(string reason) {
			DenyReason = reason;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	class S_ChatPacket : BasePacket {
		public readonly Protocol Code = Protocol.S_Chat;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string Message;

		public S_ChatPacket() {}
		public S_ChatPacket(string message) {
			Message = message;
		}
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_PeerJoinPacket : BasePacket
	{
		[FieldOffset(0)] public readonly Protocol Code = Protocol.S_PeerJoined;
		[FieldOffset(1)] public Guid PeerGUID;

		public S_PeerJoinPacket() { }
		public S_PeerJoinPacket(Guid newPeerGUID) {
			PeerGUID = newPeerGUID;
		}
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_PeerLeftPacket : BasePacket
	{
		[FieldOffset(0)] public readonly Protocol Code = Protocol.S_PeerLeft;
		[FieldOffset(1)] public Guid PeerGUID;

		public S_PeerLeftPacket() { }
		public S_PeerLeftPacket(Guid peerGUID) {
			PeerGUID = peerGUID;
		}
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_AssignPlayerIDPacket : BasePacket
	{
		[FieldOffset(0)] public readonly Protocol Code = Protocol.S_AssignPlayerID;
		[FieldOffset(1)] public short PlayerID;

		public S_AssignPlayerIDPacket() { }
		public S_AssignPlayerIDPacket(short id) {
			PlayerID = id;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	class S_AddPlayerPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.S_AddPlayer;
		public byte Red;
		public byte Green;
		public byte Blue;
		public short EntityID;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string Nickname;

		public S_AddPlayerPacket() { }
		public S_AddPlayerPacket(short entityID, byte r, byte b, byte g, string name) {
			EntityID = entityID;
			Red = r;
			Green = g;
			Blue = b;
			Nickname = name;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class S_AddBulletPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.S_AddBullet;
		public BulletDirection Direction;
		public short EntityID;
		public short CreatorID;

		public S_AddBulletPacket() { }
		public S_AddBulletPacket(short entityID, short creatorID, BulletDirection direction) {
			EntityID = entityID;
			CreatorID = creatorID;
			Direction = direction;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class S_DeleteEntityPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.S_DeleteEntity;
		public short EntityID;

		public S_DeleteEntityPacket() { }
		public S_DeleteEntityPacket(short id) {
			EntityID = id;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class S_PingPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.S_Ping;

		public S_PingPacket() { }
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class S_PingReplyPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.S_PingReply;

		public S_PingReplyPacket() {}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class S_GameStateHeader : IPacketHeader
	{
		public readonly Protocol Code = Protocol.S_GameState;
		public int PhysicsStep;

		public S_GameStateHeader() { }
		public S_GameStateHeader(int step) {
			PhysicsStep = step;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	struct S_GameStateSlice : IDataSlice
	{
		public short EntityID;
		public float NextX;
		public float NextY;
		public float VelocityX;
		public float VelocityY;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	class S_MapHeader : IPacketHeader
	{
		public readonly Protocol Code = Protocol.S_MapData;
		public S_MapHeader() { }
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	struct S_MapSlice : IDataSlice
	{
		[FieldOffset(0)] public short X;
		[FieldOffset(2)] public short Y;
		[FieldOffset(4)] public short Width;
		[FieldOffset(6)] public short Height;
		[FieldOffset(8)] public byte R;
		[FieldOffset(9)] public byte G;
		[FieldOffset(10)] public byte B;
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_LeaderboardLayoutHeader : IPacketHeader
	{
		[FieldOffset(0)] public readonly Protocol Code = Protocol.S_LeaderboardLayout;
		public S_LeaderboardLayoutHeader() { }
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	struct S_LeaderboardLayoutSlice : IDataSlice
	{
		public int Ping;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		public string Name;


		//public S_LeaderboardLayoutSlice() { }
	}

	class S_LeaderboardHeader : IPacketHeader
	{

	}

	#endregion

	#region Client Packets

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class C_PingReplyPacket : BasePacket
	{
		
		public readonly Protocol Code = Protocol.C_PingReply;
		public C_PingReplyPacket() { }
		public void SerializeToBuffer() { }
		public void DeserializeFromBuffer() { }
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class C_PingPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.C_Ping;
		public C_PingPacket() { }
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class C_ConnectRequestPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.C_Connect;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string Nickname;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] 
		public string Password;

		public C_ConnectRequestPacket() { }
		public C_ConnectRequestPacket(string name) {
			Nickname = name;
		}
	}


	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	class C_InputStatePacket : BasePacket
	{

		public readonly Protocol Code = Protocol.C_InputState;

		[MarshalAs(UnmanagedType.I1)] public bool Left = false;
		[MarshalAs(UnmanagedType.I1)] public bool Right = false;
		[MarshalAs(UnmanagedType.I1)] public bool Jump = false;
		[MarshalAs(UnmanagedType.I1)] public bool Shoot = false;
		[MarshalAs(UnmanagedType.I1)] public bool LookUp = false;
		[MarshalAs(UnmanagedType.I1)] public bool LookDown = false;
		
		public C_InputStatePacket() { }
		public C_InputStatePacket(bool left, bool right, bool jump, bool shoot, bool lookup, bool lookdown) {
			Left = left;
			Right = right;
			Jump = jump;
			Shoot = shoot;
			LookUp = lookup;
			LookDown = lookdown;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	class C_ChatPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.C_ChatMessage;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string Message;

		public C_ChatPacket() { }
		public C_ChatPacket(string msg) {
			Message = msg;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	class C_DisconnectPacket : BasePacket
	{
		public readonly Protocol Code = Protocol.C_Disconnect;
		public C_DisconnectPacket() { }
	}

	#endregion
	


	public enum NetCommand : byte
	{
		Ping,
		PingReply,
		GenericMessage,

	#region Client Commands
		RequestConnect,
		Disconnect,
		ChatSay,
		InputState,
		GetOnlinePlayers,
		GetServerInfo,

	#endregion

	#region Server Commands

		ReplyGetOnlinePlayers,
		ReplyGetServerInfo,

		AcceptConnectRequest,
		DenyConnectRequest,
		KickClient,
		PeerJoined,
		PeerLeft,
		BroadcastChat,
		SendMapData,
		SendExistingPeers,
		AssignPlayerID,
		AddPlayerEnt,
		AddBulletEnt,
		DeleteEntity,
		SendGameState,

		SendLeaderboardLayout,
		Leaderboard,
	#endregion
	}
}
