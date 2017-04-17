using UnityEngine;
using System.Collections;

public struct ClientState
{
	public NetKeyState keyState;
	public PlayerProperties properties;

	public ClientState(NetKeyState keyState, PlayerProperties properties) 
	{
		this.keyState = keyState;
		this.properties = properties;
	}
	
}
