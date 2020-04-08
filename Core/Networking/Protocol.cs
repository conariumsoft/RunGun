using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Networking
{
	public enum Protocol : byte
	{
		C_Connect = 0,
		C_Disconnect = 1,
		C_InputState,
		C_ChatMessage,
		C_Ping,
		C_PingReply,

		S_ConnectOK,
		S_ConnectDeny,
		S_Kick,
		S_Chat,
		S_PeerJoined,
		S_PeerLeft,
		S_GameState,
		S_MapData,
		S_AddPlayer,
		S_AddBullet,
		S_DeleteEntity,
		S_Leaderboard,
		S_LeaderboardLayout,
		S_Ping,
		S_PingReply,
		S_AssignPlayerID,

		S_RCON_Reply,
		RCON_Authenticate,
		RCON_Command,

		GenericMessage,
	}
}
