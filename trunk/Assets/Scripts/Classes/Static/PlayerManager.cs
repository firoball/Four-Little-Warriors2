using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public static class PlayerManager 
{
	private static List<PlayerIdentity> m_players = new List<PlayerIdentity>();

	public static List<PlayerIdentity> Players 
	{
		get {return m_players;}
	}

	static public void Add(GameObject player)
	{
		PlayerIdentity playerIdentity = player.GetComponent<PlayerIdentity>();
		if (playerIdentity != null)
		{
			m_players.Add (playerIdentity);
		}
		else
		{
			Debug.LogWarning("PlayerManager.Add: " + player.name + " does not have a PlayerIdentity.");
		}
	}

	static public void Remove (GameObject player)
	{
		PlayerIdentity playerIdentity = player.GetComponent<PlayerIdentity>();
		if (playerIdentity != null)
		{
			m_players.Remove (playerIdentity);
		}
		else
		{
			Debug.LogWarning("PlayerManager.Remove: " + player.name + " does not have a PlayerIdentity.");
		}
	}

	static public List<int> GetTeamConnectionIds(int connectionId)
	{
		List<int> teamConnectionIds = new List<int>();

		//step 1: get first PlayerIdentity for given connectionId
		foreach (PlayerIdentity player in m_players)
		{
			if (player.ConnectionId == connectionId)
			{
				//step 2: find connectionIds of team players
				foreach (PlayerIdentity teamPlayer in m_players)
				{
					//pick all connections of players in same team
					if (player.Team == teamPlayer.Team)
					{
						//make sure message is only send once for each client
						if (!teamConnectionIds.Contains(teamPlayer.ConnectionId))
						{
							teamConnectionIds.Add(teamPlayer.ConnectionId);
						}
					}
				}
				//assumption: different teams cannot share same connection - just exit loop after first hit
				break;
			}
		}

		return teamConnectionIds;
	}

	static public List<PlayerIdentity> GetPlayerIdentities(int connectionId)
	{
		List<PlayerIdentity> connPlayers = new List<PlayerIdentity>();

		//pick all players which share the given connectionId
		foreach (PlayerIdentity player in m_players)
		{
			if (player.ConnectionId == connectionId)
			{
				connPlayers.Add(player);
			}
		}

		return connPlayers;
	}

	static public bool IsLocalPlayer(int connectionId, short controllerId)
	{
		foreach (PlayerIdentity player in m_players)
		{
			if (player.ConnectionId == connectionId && player.ControllerId == controllerId)
			{
				GameObject owner = player.gameObject;
				NetworkIdentity identity = owner.GetComponent<NetworkIdentity>();
				if (identity != null && identity.isLocalPlayer)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		return false;
	}

	static public GameObject GetNearestPlayer(Vector3 position)
	{
		GameObject nearestPlayer = null;
		float nearestDist = float.MaxValue;
		foreach (PlayerIdentity player in m_players)
		{
			Vector3 diff = player.transform.position - position;
			if (diff.sqrMagnitude < nearestDist)
			{
				nearestDist = diff.sqrMagnitude;
				nearestPlayer = player.gameObject;
			}
		}
		return nearestPlayer;
	}

	static public float GetNearestPlayerDist(Vector3 position)
	{
		float nearestDist = float.MaxValue;
		foreach (PlayerIdentity player in m_players)
		{
			Vector3 diff = player.transform.position - position;
			if (diff.sqrMagnitude < nearestDist)
			{
				nearestDist = diff.sqrMagnitude;
			}
		}
		return Mathf.Sqrt(nearestDist);
	}
}
