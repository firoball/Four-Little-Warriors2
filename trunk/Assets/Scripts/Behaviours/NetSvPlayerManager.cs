using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

[RequireComponent (typeof(PlayerMovement))]
[RequireComponent (typeof(PlayerInput))]
public class NetSvPlayerManager : NetworkBehaviour
{
	private PlayerMovement m_movement;
	private PlayerInput m_input;
	private Queue<NetKeyState> m_pendingInputs;
	private SortedList<int, NetKeyState> m_postponedInputs;
	private List<ServerState> m_sendBuffer;
	private bool m_attackReleased;
	public /*private*/ int m_nextId;
	private int m_messageId;

	private const int c_postponedLength = 20;
	private const int c_sendBufferLength = 3;

	void Start () 
	{
		Debug.Log("NetSvPlayerManager Start");
		m_movement = gameObject.GetComponent<PlayerMovement>();
		m_input = gameObject.GetComponent<PlayerInput>();

		m_pendingInputs = new Queue<NetKeyState>();
		m_postponedInputs = new SortedList<int, NetKeyState>();
		m_sendBuffer = new List<ServerState>();
		m_attackReleased = true;
		m_messageId = 0;
		m_nextId = m_messageId + 1;
	}
	
	void FixedUpdate ()
	{
		if (!isServer || (isServer && isClient))
		{
			return;
		}
		NetKeyState keyState;
		while(m_pendingInputs.Count > 0)
		{
			//TODO: cheat detection
			keyState = m_pendingInputs.Dequeue();
			//apply movement
			m_input.Process(keyState.inputData);
			m_messageId = keyState.messageId;			
			ProcessActions(m_input.GetInputData());
			m_movement.ProcessInputs(m_input.GetInputData(), Time.fixedDeltaTime);
			PlayerProperties properties = m_movement.GetProperties();

			//add new state to send buffer, respect limit
			while (m_sendBuffer.Count >= c_sendBufferLength)
			{
				m_sendBuffer.RemoveAt(0);
			}
			//pushing the player is not predicted - disable server reconciliation as long as pushing is active
			if (properties.isPushed) Debug.Log("INHPUSH");
			m_sendBuffer.Add (new ServerState(m_messageId, properties, keyState.inputData, properties.isPushed));
			
			LagCompensator.Register(gameObject);
		}
	}

	void OnDestroy()
	{
		LagCompensator.Unregister(gameObject);
	}

	private void ProcessActions(InputData inputData)
	{
		//add client check to avoid activation lag compensation in local multiplayer regardless of its setting
		if ((inputData.attack > 0.0f) && m_attackReleased && !isClient)
		{
			m_attackReleased = false;

			LagCompensator.Rewind(gameObject, connectionToClient);
			m_movement.ProcessActions(m_input.GetInputData());
			LagCompensator.Restore(gameObject);

			m_movement.ProcessEvents();
		}
		else
		{
			m_movement.ProcessActions(m_input.GetInputData());
			m_movement.ProcessEvents();
		}
		
		if (inputData.attack <= 0.0f)
		{
			m_attackReleased = true;
		}
	}
	
	public int postponed = 0; //debug
	public void UpdateClientMotion(int id, int inputData)
	{
		NetKeyState keyState = new NetKeyState(id, inputData);
		postponed=m_postponedInputs.Count;
		if (id == m_nextId)
		{
			//correct order, add to queue
			m_pendingInputs.Enqueue(keyState);
			m_nextId++;
		}
		else if (id > m_nextId)
		{

			//wrong order, put newer snapshots aside
			//Debug.Log("postpone " + id);
			m_postponedInputs.Add(id, keyState);
		}
		else
		{
			//snapshot was delayed too long and is of no use anymore
			Debug.LogWarning("delayed snapshot dropped " + id); 
		}

		//grab any postponed followup keystates
		while(m_postponedInputs.ContainsKey(m_nextId))
		{
			m_pendingInputs.Enqueue(m_postponedInputs[m_nextId]);
			//Debug.Log("enqueue "+nextId);
			m_postponedInputs.Remove(m_nextId);
			m_nextId++;
		}

		//skip keystate id if too many snapshots are postponed already
		if (m_postponedInputs.Count >= c_postponedLength)
		{
			//rebuild latest keystate and add to queue with lost id
			//otherwise client and server would run out of sync with their message ids
			int input = m_sendBuffer[m_sendBuffer.Count - 1].inputData;
			keyState = new NetKeyState(m_nextId, input);
			m_pendingInputs.Enqueue(keyState);

			Debug.LogWarning ("snapshot skipped " + m_nextId);
			m_nextId++;
		}
	}
	
	public ServerState GetServerStatus()
	{
		ServerState processedState;
		if (m_sendBuffer.Count > 0)
		{
			processedState = m_sendBuffer[0];
			if (m_sendBuffer.Count > 1)
			{
				m_sendBuffer.RemoveAt(0);
			}
		}
		else
		{
			processedState = new ServerState();
			processedState.Invalidate();
		}
		return processedState;
	}
	
}
