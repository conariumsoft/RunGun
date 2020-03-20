using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RunGun.Core.Networking
{
	// commands sent from server
	public enum ServerCommand : byte
	{
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
		ADD_E_PLAYER, // int EntityType, int EntityID
		DEL_ENTITY, // int EntityID
		ENTITY_POS, // short EntityID, x, y, nextX, nextY, velX, velY
		GAME_STATE, // uint physicsStep, (short entityID, float x, y, nextX, nextY, velX, velY) X entityAmount
	}

	// commands sent from the client
	public enum ClientCommand : byte
	{
		CONNECT, // string nickname
		DISCONNECT,
		SAY, // string chatMessage
		MOVE_LEFT,
		GET_ONLINE_PLAYERS,
		MOVE_RIGHT, // 1b
		MOVE_JUMP, // 1b
		SHOOT, // 1b
		MOVE_STOP_LEFT, // 1b
		MOVE_STOP_RIGHT, // 1b
		MOVE_STOP_JUMP, // 1b
		STOP_SHOOT, // 1b
		LOOK_UP, // 1b
		STOP_LOOK_UP, // 1b
		LOOK_DOWN, // 1b
		STOP_LOOK_DOWN, // 1b
		GET_SERVER_NAME,
		PING,
		PING_REPLY,
	}

}
