using UnityEngine;
using System.Collections;

public class DebugText : MonoBehaviour 
{
	private NetClPlayerManager plm;
	public GUIText output;

	// Use this for initialization
	void Start () 
	{
		//get Debug Text object
		GameObject obj = GameObject.Find("DebugGui");
		output = obj.GetComponent<GUIText>();
		output.text = "";

		//own stuff
		plm = gameObject.GetComponent<NetClPlayerManager>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (plm == null || !plm.isLocalPlayer)
			return;

		//if (Network.connections.Length == 0) 
		if(!plm.connectionToServer.isReady)
		{
			output.text = "Connection to server lost";
			return;
		}
		//quick and dirty: ignore peers
		//if (plm.GetOwner() == Network.player)
		if (plm.isLocalPlayer && !plm.isServer)
		{
			output.text = /*"dist: " + plm.distance+*/"\n"
				+ "clID: "+plm.m_messageId + "\n"
					+ "svID: "+plm.serverId + " (delta: " + (plm.m_messageId-plm.serverId) + ")\n"
					//+ "Ping: " + Network.GetAveragePing(Network.connections[0])+"\n"
					+ "Queue: "+plm.m_pendingStates.Count+"\n"
					//+ "T-ipol: "+plm.xtime+ " " +plm.xcnt
					+ "T: "+Time.fixedDeltaTime+"\n"
					;
		}
	}

}
