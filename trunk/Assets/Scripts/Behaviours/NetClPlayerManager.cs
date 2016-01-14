using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof(PlayerMovement))]
[RequireComponent (typeof(PlayerInput))]
public class NetClPlayerManager : NetworkBehaviour 
{
	private PlayerMovement m_movement;
	private PlayerInput m_input;
	private NetStreamer m_streamer;

	public/*private*/ List<ServerState> m_serverStateBuffer;
	private float m_stateTime;

	//owner data
	public /*private*/ int m_messageId;
	public/*private*/ List<ClientState> m_pendingStates;

	//peer data
	private bool m_peerInterpolationStarted = false;

	//debug
	public int serverId;

	void Awake()
	{
		m_pendingStates = new List<ClientState>();
		m_serverStateBuffer = new List<ServerState>();
		m_messageId = 0;
		m_stateTime = 0.0f;
	}

	
	void OnDestroy()
	{
		//if any hud was connected, destroy it
		HudConfiguration.DestroyHud(gameObject);
	}
	// Use this for initialization
	void Start () 
	{
		Debug.Log("NetClPlayerManager Start");

		m_movement = gameObject.GetComponent<PlayerMovement>();
		m_input = gameObject.GetComponent<PlayerInput>();
		m_streamer = gameObject.GetComponent<NetStreamer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (isLocalPlayer)
		{
			InterpolateOwner();
		}
		else if (isClient)
		{
			InterpolatePeer();
		}
	}

	void FixedUpdate () 
	{
		if (isLocalPlayer)
		{
			//store interpolated player state
			PlayerProperties properties = m_movement.GetProperties();

			//update from server - includes reconciliation
			PredictOwner();

			m_input.Process();	
			m_messageId++;
			int data = m_input.Data();
			NetKeyState keyState = new NetKeyState(m_messageId, data);

			//client prediction
			//rewind to last non interpolated state
			if (m_pendingStates.Count > 0)
			{
				ClientState cs = m_pendingStates[m_pendingStates.Count - 1];
				m_movement.SetProperties(cs.properties);
			}
//			m_movement.ProcessInputs(m_input.GetInputData(), Time.fixedDeltaTime);
			if (!isServer || (isServer && isClient))
			{
				m_movement.ProcessActions(m_input.GetInputData()); //local prediction of actions
			}
			m_movement.ProcessInputs(m_input.GetInputData(), Time.fixedDeltaTime);

			//send key state to server
			if (connectionToServer.isReady && m_streamer != null)
			{
				//DEBUG START
				/*serverid = m_messageId;
				if (m_input.shoot == true)
				{
					string ids = "";
					//Debug.Log (PlayerManager.m_players.Count);
					foreach(PlayerIdentity identity in PlayerManager.Players)
					{
						if (identity.gameObject == gameObject)
						{
							continue;
						}
						NetClPlayerManager clm = identity.gameObject.GetComponent<NetClPlayerManager>();
						if (clm.m_serverStateBuffer.Count > 0)
							ids += identity.name+": "+clm.m_serverStateBuffer[0].messageId+" ";
					}
					Debug.Log (name+": "+m_messageId +" ("+ids+")");
				}*/
				//DEBUG END
				m_streamer.CmdUpdateMotion(m_messageId, data);
			}

			//client reconciliation: add key state to pending input list
			//log curent player state for cl<->sv deviation check
			if (m_pendingStates.Count == 100)
			{
				m_pendingStates.RemoveAt(0);
			}

			ClientState clientState = new ClientState(keyState, m_movement.GetProperties());
			m_pendingStates.Add(clientState);

			//prepare for interpolation - rewind pos/rot to interpolated state
			m_movement.SetProperties(properties);
			m_stateTime = Time.time;
		}
	}

	public override void OnStartLocalPlayer()
	{
		HudConfiguration.CreateHud(gameObject);

		//sync message id with server
		/*PlayerIdentity identity = GetComponent<PlayerIdentity>();
		if (identity != null)
		{
			m_messageId = identity.ServerSyncId;
		}*/
		
		gameObject.name += " (Owner)";
	}

	public void ReportServerStatus(ServerState state)
	{
		//don't queue duplicate states sent from server - will destroy peer interpolation
		if ((m_serverStateBuffer.Count > 0) &&
			(state.messageId == m_serverStateBuffer[m_serverStateBuffer.Count - 1].messageId)
		    )
		{
			return;
		}

		m_serverStateBuffer.Add(state);
	}

	public int svbuffer = 0;
	private void PredictOwner()
	{
		ServerState serverState;
		svbuffer=m_serverStateBuffer.Count;
		if (m_serverStateBuffer.Count > 0)
		{
			serverState = m_serverStateBuffer[m_serverStateBuffer.Count - 1];
			serverId = serverState.messageId; //debug

			if ((m_pendingStates.Count > 0))
			{

				//remove all old input states but the newest two - they are required for interpolation
				while ((m_pendingStates.Count > 2) && (serverState.messageId >= m_pendingStates[0].keyState.messageId))
				{
					/*if (
						(serverState.messageId == m_pendingStates[0].keyState.messageId) &&
						serverState.inhibitReconciliation
						)
					{
						Debug.Log("inhibited");
					}*/
						//update position with server data
					if (
						(serverState.messageId == m_pendingStates[0].keyState.messageId) &&
						!serverState.inhibitReconciliation &&
						(serverState.properties != m_pendingStates[0].properties)
					 )
					{
						//Debug.Log ("update pos " + serverState.messageId + " " + m_pendingStates[0].keyState.messageId);
						m_movement.SetProperties(serverState.properties);
						m_pendingStates.RemoveAt(0);
						//re-apply all remaining states
						if (m_pendingStates.Count > 0)
						{
							for (int i = 0; i < m_pendingStates.Count; i++)
							{
								ClientState clientState = m_pendingStates[i];
								NetKeyState keyState = clientState.keyState;
								m_input.Process(keyState.inputData);
								m_movement.ProcessInputs(m_input.GetInputData(), Time.fixedDeltaTime);
								clientState.properties = m_movement.GetProperties();
								m_pendingStates[i] = clientState;
							}
						}
					}
					else
					{
						m_pendingStates.RemoveAt(0);
					}
				} 
			}
			m_serverStateBuffer.Clear();
		}
	}

	private void InterpolatePeer()
	{
		if (m_serverStateBuffer == null)
		{
			return;
		}
		
		/* interpolation cannot be started until three fixed frames are available
		 * the third frame is used to compensate network delays and avoid constant
		 * minimum extrapolation
		 * Since server is calculating player states with fixed update rate, it is
		 * possible to calculate delta times by using message ids of each state.
		 */
		if (m_serverStateBuffer.Count > 2)
		{			
			if (!m_peerInterpolationStarted)
			{
				m_stateTime = Time.time;
				m_peerInterpolationStarted = true;
				//update data required by animation
				m_movement.SetProperties(m_serverStateBuffer[0].properties);
				m_input.Process(m_serverStateBuffer[0].inputData);
			}
			
			ServerState svOld = m_serverStateBuffer[0];
			ServerState svCur = m_serverStateBuffer[1];
			float timePassed = Time.time - m_stateTime;
			float timeRange = (svCur.messageId - svOld.messageId) * Time.fixedDeltaTime;

			//distance of first pair covered - proceed with next pair 
			if (timePassed > timeRange)
			{
				svOld = m_serverStateBuffer[1];
				svCur = m_serverStateBuffer[2];
				timePassed -= timeRange;
				timeRange = (svCur.messageId - svOld.messageId) * Time.fixedDeltaTime;

				//check if state buffer is well filled
				if (m_serverStateBuffer.Count > 3)
				{
					m_stateTime = Time.time;
					//try smooth transition if only one more state is queued
					if (m_serverStateBuffer.Count == 4)
					{
						m_stateTime -= timePassed;
					}
					//make sure delay doesn't become too much - always limit to 3 states
					while (m_serverStateBuffer.Count > 3)
					{
						m_serverStateBuffer.RemoveAt(0);
					}
					//update data required by animation
					m_movement.SetProperties(m_serverStateBuffer[0].properties);
					m_input.Process(m_serverStateBuffer[0].inputData);
				}				
			}

			float time = timePassed / timeRange;
			m_movement.ProcessInterpolation(svOld.properties, svCur.properties, time, true);
		}
	}
	public int pending	= 0;
	private void InterpolateOwner()
	{
		pending=m_pendingStates.Count;
		//interpolation cannot be started until two fixed frames are available
		if (m_pendingStates.Count > 1)
		{
			//ClientState cs;
			float time = (Time.time - m_stateTime) / Time.fixedDeltaTime;
			time = Mathf.Max(Mathf.Min(time, 1.0f), 0.0f);
			
			ClientState csOld = m_pendingStates[m_pendingStates.Count - 2];
			ClientState csCur = m_pendingStates[m_pendingStates.Count - 1];
			m_movement.ProcessInterpolation(csOld.properties, csCur.properties, time);
		}
		// http://www.gamedev.net/topic/608997-somewhat-jerky-movement-when-doing-client-side-prediction/
	}
}
