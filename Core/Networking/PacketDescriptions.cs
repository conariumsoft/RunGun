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
using RunGun.Core.Game;
using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
		[FieldOffset(0)] public readonly new Protocol Code = Protocol.S_PeerJoined;
		[FieldOffset(1)] public Guid PeerGUID;

		public S_PeerJoinPacket() { }
		public S_PeerJoinPacket(Guid newPeerGUID) {
			PeerGUID = newPeerGUID;
		}
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_PeerLeftPacket : BasePacket
	{
		[FieldOffset(0)] public readonly new Protocol Code = Protocol.S_PeerLeft;
		[FieldOffset(1)] public Guid PeerGUID;

		public S_PeerLeftPacket() { }
		public S_PeerLeftPacket(Guid peerGUID) {
			PeerGUID = peerGUID;
		}
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_AssignPlayerIDPacket : BasePacket
	{
		[FieldOffset(0)] public readonly new Protocol Code = Protocol.S_AssignPlayerID;
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
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	class S_GameStateHeader : IPacketHeader
	{
		[FieldOffset(0)] public readonly Protocol Code = Protocol.S_GameState;
		[FieldOffset(1)] public int PhysicsStep;

		public S_GameStateHeader() { }
		public S_GameStateHeader(int step) {
			PhysicsStep = step;
		}
	}
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	struct S_GameStateSlice : IDataSlice
	{
		[FieldOffset(0)] public short EntityID;
		[FieldOffset(2)] public float NextX;
		[FieldOffset(6)] public float NextY;
		[FieldOffset(10)] public float VelocityX;
		[FieldOffset(14)] public float VelocityY;
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
	/*	
		public struct ConnectAcceptPacket : IPacket
		{
			public NetCommand Code { get; set; }
			public Guid UserAssignedGUID { get; set; }
			public int InitialPhysicsStep { get; set; }
			public ConnectAcceptPacket(Guid id, int physStep) {
				Code = NetCommand.AcceptConnectRequest;
				UserAssignedGUID = id;
				InitialPhysicsStep = physStep;
			}
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct ConnectDenyPacket : IPacket
		//
		{
			public NetCommand Code { get; set; }
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public string DenialReason;
			public ConnectDenyPacket(string reason) {
				Code = NetCommand.DenyConnectRequest;
				DenialReason = reason;
			}
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct KickPacket : IPacket
		//
		{
			public NetCommand Code { get; set; }
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public string KickReason;
			public KickPacket(string reason) {
				Code = NetCommand.KickClient;
				KickReason = reason;
			}
		}
		public struct PeerJoinedPacket : IPacket
		//
		{
			
		}
		public struct PeerLeftPacket : IPacket
		//
		{
			
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		class S_ChatPacket : BasePacket
		{
			
		}*/


	/*public struct PingPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public short Unused { get; set; }
		public PingPacket(short _) {
			Code = NetCommand.Ping;
			Unused = 0;
		}
	}
	public struct PingReplyPacket : IPacket
	//
	{
		public NetCommand Code { get; set; }
		public short Unused { get; set; }
		public PingReplyPacket(short _) {
			Code = NetCommand.PingReply;
			Unused = 0;
		}
	}
	public struct AssignPlayerIDPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public short YourPlayerID { get; set; }
		public AssignPlayerIDPacket(short id) {
			Code = NetCommand.AssignPlayerID;
			YourPlayerID = id;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct AddPlayerEntPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public short EntityID { get; set; }
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string Nickname;
		public AddPlayerEntPacket(short entityID, byte r, byte b, byte g, string nick) {
			Code = NetCommand.AddPlayerEnt;
			EntityID = entityID;
			R = r;
			G = g;
			B = b;
			Nickname = nick;
		}
	}
	public struct AddBulletEntPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public short EntityID { get; set; }
		public short CreatorID { get; set; }
		public BulletDirection Direction { get; set; }
		public AddBulletEntPacket(short entityID, short creatorID, BulletDirection direction) {
			Code = NetCommand.AddBulletEnt;
			EntityID = entityID;
			CreatorID = creatorID;
			Direction = direction;
		}
	}
	public struct DeleteEntityPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public short EntityID { get; set; }

		public DeleteEntityPacket(short entityID) {
			EntityID = entityID;
			Code = NetCommand.DeleteEntity;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ReplyServerInfoPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public int MaxPlayers { get; set; }
		public int PlayersOnline { get; set; }
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string ServerName;

		public ReplyServerInfoPacket(int maxPlayers, int playersOnline, string serverName) {
			Code = NetCommand.ReplyGetServerInfo;
			MaxPlayers = maxPlayers;
			PlayersOnline = playersOnline;
			ServerName = serverName;
		}
	}

	#region Leaderboard
	public struct LeaderboardLayoutHeader : IHeader
	{
		public NetCommand Code { get; set; }

		public LeaderboardLayoutHeader(short _) {
			Code = NetCommand.SendLeaderboardLayout;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct LeaderboardColumnData : IPayload
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string Label;
	}
	#endregion

	#region Map Download
	public struct MapDataHeader : IHeader
	{
		public NetCommand Code { get; set; }

		public MapDataHeader(short _) {
			Code = NetCommand.SendMapData;
		}
	}
	public struct GeometryData : IPayload
	{
		public short X { get; set; }
		public short Y { get; set; }
		public short Width { get; set; }
		public short Height { get; set; }
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }

		public GeometryData(LevelGeometry geom) {
			X = (short)geom.Position.X;
			Y = (short)geom.Position.Y;
			Width = (short)geom.Size.X;
			Height = (short)geom.Size.Y;
			R = geom.Color.R;
			G = geom.Color.G;
			B = geom.Color.B;
		}
	}
	#endregion

	#region Game State
	public struct GameStateHeader : IHeader
	{
		public NetCommand Code { get; set; }
		public int PhysicsFrame { get; set; }
		public GameStateHeader(int physFrame) {
			Code = NetCommand.SendGameState;
			PhysicsFrame = physFrame;
		}
	}
	public struct EntityStateData : IPayload
	{
		public short EntityID { get; set; }
		public float NextX { get; set; }
		public float NextY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
	}
	#endregion

	#region Online Players List
	public struct PlayerListHeader : IHeader
	{
		public NetCommand Code { get; set; }

		public PlayerListHeader(short _) {
			 Code = NetCommand.ReplyGetOnlinePlayers;
		}
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct PlayerListEntry : IPayload
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string PlayerName;
	}

	#endregion

	#endregion

	#region ClientPackets

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	struct RequestConnectPacket : IPacket
	{
		public NetCommand Code { get; set; }

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string RequestedNickname;

		public RequestConnectPacket(string name) {
			Code = NetCommand.RequestConnect;
			RequestedNickname = name;
		}
	}
	struct DisconnectPacket : IPacket
	{
		public NetCommand Code { get; set; }

		public DisconnectPacket(short _) {
			Code = NetCommand.Disconnect;
		}
	}
	struct UserSendChatPacket : IPacket
	{
		public NetCommand Code { get; set; }

		public UserSendChatPacket(short _) {
			 Code = NetCommand.ChatSay;
		}
	}
	struct UserInputStatePacket : IPacket
	{
		public NetCommand Code { get; set; }
		public bool Left { get; set; }
		public bool Right { get; set; }
		public bool Jumping { get; set; }
		public bool Shooting { get; set; }
		public bool LookUp { get; set; }
		public bool LookDown { get; set; }

		public UserInputStatePacket(bool left, bool right, bool jump, bool shooting, bool lookUp, bool lookDown) {
			Code = NetCommand.InputState;
			Left = left;
			Right = right;
			Jumping = jump;
			Shooting = shooting;
			LookUp = lookUp;
			LookDown = lookDown;
		}
	}
	struct GetServerInfoPacket : IPacket
	{
		public NetCommand Code { get; set; }

		public GetServerInfoPacket(short _) {
			 Code = NetCommand.GetServerInfo;
		}
	}
	struct GetOnlinePlayersPacket : IPacket {
		public NetCommand Code { get; set; }

		public GetOnlinePlayersPacket(short _) {
			Code = NetCommand.GetOnlinePlayers;
		}
	}
	struct UserChatPacket : IPacket
	{
		public NetCommand Code { get; set; }
		public string Message { get; set; }
		public UserChatPacket(string msg) {
			Code = NetCommand.ChatSay;
			Message = msg;
		}
	}
	#endregion*/


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
