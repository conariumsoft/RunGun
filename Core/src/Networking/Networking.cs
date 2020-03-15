using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RunGun.Core.Networking
{
	

	// commands sent from server
	public enum ServerCommand : int {
		CONNECT_OK,
		CONNECT_DENY, // string denialReason
		KICK, // 
		USER_JOINED, //
		USER_LEFT, // string reason
		CHAT_MSG, // 
		SEND_MAP_DATA,
		EXISTING_USER, // send current connections to new clients
		PLAYER_POS,
		PONG,
	}

	// commands sent from the client
	public enum ClientCommand: int
	{
		CONNECT, // string nickname
		DISCONNECT, 
		SAY, // string chatMessage
		PLR_LEFT,
		GET_ONLINE_PLAYERS,
		PLR_RIGHT,
		PLR_JUMP,
		PLR_SHOOT,
		PLR_STOP_LEFT,
		PLR_STOP_RIGHT,
		PLR_STOP_JUMP,
		PLR_STOP_SHOOT,
		GET_SERVER_NAME,
		PING,
		PONG,
	}
}
