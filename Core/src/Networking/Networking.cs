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
		PING,
		PING_REPLY,
		YOUR_PID, // tell the client what their playerID is.
		ADD_ENTITY, // int EntityType, int EntityID
		DEL_ENTITY, // int EntityID
		ENTITY_POS, // int EntityID, x, y, nextX, nextY, velX, velY
	}

	// commands sent from the client
	public enum ClientCommand: int
	{
		CONNECT, // string nickname
		DISCONNECT, 
		SAY, // string chatMessage
		MOVE_LEFT,
		GET_ONLINE_PLAYERS,
		MOVE_RIGHT,
		MOVE_JUMP,
		SHOOT,
		MOVE_STOP_LEFT,
		MOVE_STOP_RIGHT,
		MOVE_STOP_JUMP,
		STOP_SHOOT,
		GET_SERVER_NAME,
		PING,
		PING_REPLY,
	}
}
