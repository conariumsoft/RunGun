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
using System;

namespace RunGun.Core.Networking
{

	#region Server Packets
	class SPConnectAccept : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_ConnectOK;
		[Schema] public Guid UserGUID;

		public SPConnectAccept() { }
		public SPConnectAccept(Guid userGuid) {
			UserGUID = userGuid;
		}
	}

	class SPConnectDeny : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_ConnectDeny;
		[Schema] public string DenyReason;
		public SPConnectDeny() { }
		public SPConnectDeny(string reason) {
			DenyReason = reason;
		}
	}

	class SPChat : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_Chat;
		[Schema(64)] public string Message;
		public SPChat() { }
		public SPChat(string message) {
			Message = message;
		}
	}

	class SPPeerJoin : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_PeerJoined;
		[Schema] public Guid PeerGUID;
		public SPPeerJoin() { }
		public SPPeerJoin(Guid newPeerGUID) {
			PeerGUID = newPeerGUID;
		}
	}

	class SPPeerLeft : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_PeerLeft;
		[Schema] public Guid PeerGUID;
		public SPPeerLeft() { }
		public SPPeerLeft(Guid peerGUID) {
			PeerGUID = peerGUID;
		} 
	}

	class SPAssignPlayerID : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_AssignPlayerID;
		[Schema] public short PlayerID;

		public SPAssignPlayerID() { }
		public SPAssignPlayerID(short id) {
			PlayerID = id;
		}
	}

	class SPAddPlayer : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_AddPlayer;
		[Schema] public byte Red;
		[Schema] public byte Green;
		[Schema] public byte Blue;
		[Schema] public short EntityID;
		[Schema(16)] public string Nickname;
		public SPAddPlayer() { }
		public SPAddPlayer(short entityID, byte r, byte b, byte g, string name) {
			EntityID = entityID;
			Red = r;
			Green = g;
			Blue = b;
			Nickname = name;
		}
	}

	class SPAddBullet : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_AddBullet;
		[Schema] public BulletDirection Direction;
		[Schema] public short EntityID;
		[Schema] public short CreatorID;

		public SPAddBullet() { }
		public SPAddBullet(short entityID, short creatorID, BulletDirection direction) {
			EntityID = entityID;
			CreatorID = creatorID;
			Direction = direction;
		}
	}
	class SPDeleteEntity : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_DeleteEntity;
		[Schema] public short EntityID;

		public SPDeleteEntity() { }
		public SPDeleteEntity(short id) {
			EntityID = id;
		}
	}

	class SPPing : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_Ping;
		public SPPing() { }
	}
	class SPPingReply : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.S_PingReply;

		public SPPingReply() {}
	}

	class SGameStateHeader : IPacketHeader
	{
		[Schema] public readonly Protocol Code = Protocol.S_GameState;
		[Schema] public int PhysicsStep;

		public SGameStateHeader() { }
		public SGameStateHeader(int step) {
			PhysicsStep = step;
		}
	}
	class SGameStateSlice : IDataSlice
	{
		[Schema] public short EntityID;
		[Schema] public float NextX;
		[Schema] public float NextY;
		[Schema] public float VelocityX;
		[Schema] public float VelocityY;
		[Schema] public byte Flag0 = 0;
		[Schema] public byte Flag1 = 0;
	}
	class SMapHeader : IPacketHeader
	{
		[Schema] public readonly Protocol Code = Protocol.S_MapData;
		public SMapHeader() { }
	}
	class SMapSlice : IDataSlice
	{
		[Schema] public short X;
		[Schema] public short Y;
		[Schema] public short Width;
		[Schema] public short Height;
		[Schema] public byte R;
		[Schema] public byte G;
		[Schema] public byte B;
	}
	class SLeaderboardLayoutHeader : IPacketHeader
	{
		[Schema] public readonly Protocol Code = Protocol.S_LeaderboardLayout;
		public SLeaderboardLayoutHeader() { }
	}
	class SLeaderboardLayoutSlice : IDataSlice
	{
		[Schema] public int Ping;
		[Schema(16)] public string Name;
		//public S_LeaderboardLayoutSlice() { }
	}

	class SLeaderboardHeader : IPacketHeader
	{

	}

	#endregion

	#region Client Packets

	class CPingReply : BasePacket
	{
		
		[Schema] public readonly Protocol Code = Protocol.C_PingReply;
		public CPingReply() { }
	}

	class CPing : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.C_Ping;
		public CPing() { }
	}

	class CConnectRequest : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.C_Connect;
		[Schema(16)] public string Nickname;
		//[Schema(32)] public string Password;

		public CConnectRequest() { }
		public CConnectRequest(string name) {
			Nickname = name;
		}
	}

	class CInputState : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.C_InputState;
		[Schema] public bool Left = false;
		[Schema] public bool Right = false;
		[Schema] public bool Jump = false;
		[Schema] public bool Shoot = false;
		[Schema] public bool LookUp = false;
		[Schema] public bool LookDown = false;
		
		public CInputState() { }
		public CInputState(bool left, bool right, bool jump, bool shoot, bool lookup, bool lookdown) {
			Left = left;
			Right = right;
			Jump = jump;
			Shoot = shoot;
			LookUp = lookup;
			LookDown = lookdown;
		}
	}

	class CChat : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.C_ChatMessage;
		[Schema(64)] public string Message;

		public CChat() { }
		public CChat(string msg) {
			Message = msg;
		}
	}
	class CDisconnect : BasePacket
	{
		[Schema] public readonly Protocol Code = Protocol.C_Disconnect;
		public CDisconnect() { }
	}

	#endregion

}
