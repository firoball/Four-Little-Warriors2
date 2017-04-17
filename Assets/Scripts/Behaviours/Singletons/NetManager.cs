using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetManager : NetworkManager
{
	[SerializeField]
	private List<GameObject> m_playerTracker;

	void Start () 
	{
		m_playerTracker = new List<GameObject>();
	}

	/* -----------
	 * Server side
	 * ----------- */

	public override void OnStartServer()
	{
		ChatServer.Initialize();
		base.OnStartServer();
	}

	public override void OnServerConnect (NetworkConnection conn)
	{
		Debug.Log("Client connected: " + conn.connectionId);
		base.OnServerConnect(conn);
	}

	public override void OnServerDisconnect (NetworkConnection conn)
	{
		Debug.Log("Client disconnected: " + conn.connectionId);
		base.OnServerDisconnect(conn);
	}
	
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		Debug.Log("NetManager OnServerAddPlayer");
		Vector3 position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		PlayerSpawnPoint spawnPoint = PlayerSpawnPoint.Next();
		if (spawnPoint != null)
		{
			position = spawnPoint.position;
			rotation = spawnPoint.rotation;
		}

		//add player object
		GameObject player = (GameObject)GameObject.Instantiate(playerPrefab, position, rotation);
		PlayerIdentity identity = player.GetComponent<PlayerIdentity>();
		if (identity != null)
		{
			//TODO: player name and team id values are temporary - maybe grab from settings?
			string playerName = player.name + conn.connectionId.ToString();
			byte teamId = 1;
			identity.Initialize(conn.connectionId, playerControllerId, playerName, teamId);
		}
		else
		{
			Debug.LogWarning("NetManager: Player prefab does not have a PlayerIdentity assigned.");
		}
		m_playerTracker.Add(player);
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
	}

	public override void OnServerRemovePlayer (NetworkConnection conn, PlayerController player)
	{
		m_playerTracker.Remove(player.gameObject);
		base.OnServerRemovePlayer(conn, player);
	}

	public override void OnStopServer ()
	{
		foreach (GameObject player in m_playerTracker)
		{
			NetworkServer.Destroy (player);
		}
		m_playerTracker.Clear();
	}

	public override void OnStopHost ()
	{
		OnStopServer();
	}
	
	/* -----------
	 * Client side
	 * ----------- */

	public override void OnClientConnect(NetworkConnection conn)
	{
		ClientScene.Ready(conn);
		if (NetworkServer.active && NetworkServer.localClientActive)
		{
			for (int i = 0; i < Settings.localPlayers; i++)
			{
				ClientScene.AddPlayer(Convert.ToInt16(i));
			}
		}
		else
		{
			ClientScene.AddPlayer(Convert.ToInt16(0));
			ChatClient.Initialize();
		}
	}

	public override void OnClientDisconnect (NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		Debug.LogWarning("Client network error #"+ errorCode +": " + conn.connectionId);
	}
	
	public override void OnClientNotReady(NetworkConnection conn)
	{
		Debug.LogWarning("Client not ready: " + conn.connectionId);
	}

}
