using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerIdentity : NetworkBehaviour 
{
	[ShowOnly][SerializeField][SyncVar]
	private int m_connectionId;
	[ShowOnly][SerializeField][SyncVar]
	private short m_controllerId;
	[ShowOnly][SerializeField][SyncVar]
	private byte m_team;
	[ShowOnly][SerializeField][SyncVar]
	private string m_name;

	public int ConnectionId 
	{
		get {return m_connectionId;}
	}

	public short ControllerId 
	{
		get {return m_controllerId;}
	}
	
	public byte Team 
	{
		get {return m_team;}
	}

	public string Name 
	{
		get {return m_name;}
	}
		
	public override void OnStartClient()
	{
		//local multiplayer may not have its own client PlayerManager
		if (!isServer)
		{
			Debug.Log ("PlayerIdentity OnStartClient");
			PlayerManager.Add(gameObject);
			name = "Player id: " + m_connectionId + "_" + m_controllerId;
		}
	}

	public override void OnStartServer()
	{
		Debug.Log ("PlayerIdentity OnStartServer");
		PlayerManager.Add(gameObject);
		name = "Player id: " + m_connectionId + "_" + m_controllerId;
	}
	
	void OnDestroy()
	{
		Debug.Log ("PlayerIdentity OnDestroy");
		PlayerManager.Remove(gameObject);
	}

	public void Initialize(int connectionId, short controllerId, string name, byte team = 0)
	{
		//isServer is not in correct state (object not yet spawned), so check NetworkServer
		if (!NetworkServer.active)
		{
			return;
		}
		Debug.Log("PlayerIdentity Initialize");
		m_connectionId = connectionId;
		m_controllerId = controllerId;
		m_name = name;
		m_team = team;
	}
}
