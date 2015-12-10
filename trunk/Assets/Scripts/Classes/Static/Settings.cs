using UnityEngine;
using System.Collections;

public static class Settings
{
	public static GameMode gameMode = GameMode.undefined;
	public static GameRules gameRules = GameRules.undefined;
	public static int maxPlayers = 4;
	public static int localPlayers = 4;//4;

	/*public static bool isLocalGame
	{
		get { return (gameMode == GameMode.local);}
	}

	public static bool isNetworkGame
	{
		get { return (gameMode == GameMode.network);}
	}
	
	public static bool isHost
	{
		get { return ((isNetworkGame && Network.isServer) || isLocalGame);}
	}*/

}

public enum GameRules : int
{
	undefined = 0,
	crystalHunt = 1,
	lastManStanding = 2,
	frags = 3,
	creatureSlayer = 4
}

public enum GameMode : int
{
	undefined = 0,
	local = 1,
	network = 2
}

