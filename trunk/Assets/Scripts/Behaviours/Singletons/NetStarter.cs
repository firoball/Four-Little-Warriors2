using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class NetStarter : MonoBehaviour 
{

	private string ip = "localhost";
	private int port = 25000;
	//private int maxplayers = 4;
	//private bool menuActive = true;
	NetManager networkManager;
	private bool server = false;
	private bool client = false;
	private bool host = false;
	NetworkClient ncl;
	private string splayers = "1";
	private int players = 1;

	// Use this for initialization
	void Start () 
	{
		networkManager = (NetManager)NetManager.singleton;
		//temp
		networkManager.networkAddress = ip;
		networkManager.networkPort = port;
		networkManager.maxConnections = Settings.maxPlayers;
	}
	
	void OnGUI()
	{
		//if(!menuActive)
		if (networkManager.isNetworkActive)
		{
			int connections = 0;// = networkManager.connections.Count;
			int bytesIn = 0;
			int bytesOut = 0;
			int totalBytesIn = 0;
			int totalBytesOut = 0;
			int msg;
			string ping = "";
			string button = "";
			byte error;
			/*if (!host)
			{
				foreach (NetworkConnection conn in networkManager.connections)
				{
					bytesIn += NetworkTransport.GetPacketReceivedRate(conn.hostId, conn.connectionId, out error);
					bytesOut += NetworkTransport.GetPacketSentRate(conn.hostId, conn.connectionId, out error);
					ping += (NetworkTransport.GetCurrentRtt(conn.hostId, conn.connectionId, out error) / 2) + "ms ";
				}
			}*/
			if (server || host)
			{
				//connections = NetworkServer.connections.Count;
				foreach (NetworkConnection conn in NetworkServer.connections)// networkManager.connections)
				{
					if (conn != null) //dozey - is this "own" connection?
					{
						connections++;
						bytesIn += NetworkTransport.GetPacketReceivedRate(conn.hostId, conn.connectionId, out error);
						bytesOut += NetworkTransport.GetPacketSentRate(conn.hostId, conn.connectionId, out error);
						ping += (NetworkTransport.GetCurrentRtt(conn.hostId, conn.connectionId, out error) / 2) + "ms ";
					}
				}
				NetworkServer.GetStatsIn(out msg, out totalBytesIn);
				NetworkServer.GetStatsOut(out msg, out msg, out totalBytesOut, out msg);
				button = "Stop Server";
			}
			else
			{
				NetworkConnection conn = ncl.connection;
				connections = 1;
				bytesIn += NetworkTransport.GetPacketReceivedRate(conn.hostId, conn.connectionId, out error);
				bytesOut += NetworkTransport.GetPacketSentRate(conn.hostId, conn.connectionId, out error);
				ping += (NetworkTransport.GetCurrentRtt(conn.hostId, conn.connectionId, out error) / 2) + "ms ";
				ncl.GetStatsIn(out msg, out totalBytesIn);
				ncl.GetStatsOut(out msg, out msg, out totalBytesOut, out msg);
				button = "Disconnect";
			}

			GUI.Label(new Rect(10, 10, 200, 70), 
			          "conn: " + connections + 
			          "\nin: " + bytesIn + "/s total: " + totalBytesIn + 
			          "\nout: " + bytesOut +  "/s total: " + totalBytesOut +
			          "\nping: " + ping
			          );
			if (GUI.Button(new Rect(10, 80, 100, 25), button)) 
			{
				if (server) 
				{
					networkManager.StopServer();
					server = false;
				}
				if (client) 
				{
					networkManager.StopClient();
					client = false;
				}
				if (host) 
				{
					networkManager.StopHost();
					host = false;
				}
			}
			return;
		}
		else
		{
			if (GUI.Button(new Rect(10, 10, 100, 25), "Start a Server")) 
			{
				//menuActive = false;
				Settings.gameMode = GameMode.network;
				//Network.InitializeServer(maxplayers, port, !Network.HavePublicAddress());
				//ConnectionConfig connectionCfg = new ConnectionConfig();
	//			networkManager.StartServer(connectionCfg, Settings.maxPlayers);
				networkManager.maxConnections = Settings.maxPlayers;
				networkManager.StartServer();
				server = true;
			}
			
			if (GUI.Button(new Rect(10, 50, 150, 25), "Connect to a Server ...")) 
			{
				//menuActive = false;
				Settings.gameMode = GameMode.network;
				Settings.localPlayers = 1;
				//Network.Connect(ip, port);
				networkManager.maxConnections = 1;
				ncl = networkManager.StartClient();
				client = true;
			}

			string old = splayers;
			splayers = GUI.TextField(new Rect(170, 90, 30, 25), splayers, 1);
			if (splayers.Length > 0)
			{
				int x;
				if(!Int32.TryParse(splayers, out x))
				{
					splayers = old;
				}
				else
				{
					players = Math.Min(x, Settings.maxPlayers);
					players = Math.Max(players, 1);
					splayers = players.ToString();
				}
			}
				

			if (GUI.Button(new Rect(10, 90, 150, 25), "Local Multiplayer")) 
			{
				//menuActive = false;
				Settings.gameMode = GameMode.local;
				Settings.localPlayers = players;
				//Settings.maxPlayers = 4;
				//EventHandler.TriggerLocalGameInitialized();
				//ConnectionConfig connectionCfg = new ConnectionConfig();
				//networkManager.StartHost(connectionCfg, 1);
				ncl = networkManager.StartHost();
				host = true;
			}
		}
	}

}
