using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Networking
{
	enum NetMsg {
		PING, // 
		PONG, //
		DISCONNECT,
		CONNECT,
		CONNECT_ACK,
		CHAT,
		DL_LEVEL_GEOMETRY,
		C_LEFT_DOWN,
		C_LEFT_UP,
		C_RIGHT_DOWN,
		C_RIGHT_UP,
		C_JUMP_DOWN,
		C_JUMP_UP,
		C_SHOOT_DOWN,
		C_SHOOT_UP,
		PEER_JOINED,
		PEER_LEFT,
		PLAYER_POS,
		ERR,
		EXISTING_PEER,
	}
}
