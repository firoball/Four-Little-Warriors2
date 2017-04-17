using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[NetworkSettings(channel=1,sendInterval=0.05f)]
public class NetStreamer : NetworkBehaviour 
{
	private NetClPlayerManager m_receiver = null;
	private NetSvPlayerManager m_sender = null;
	private ServerState m_stateToSend;

	void Awake()
	{
		Debug.Log("NetStreamer Awake");
		m_receiver = gameObject.GetComponent<NetClPlayerManager>();
		m_sender = gameObject.GetComponent<NetSvPlayerManager>();
		m_stateToSend.Invalidate();
	}

	void Update()
	{
		if (isServer && m_sender != null)
		{
			m_stateToSend = m_sender.GetServerStatus();
			if (m_stateToSend.isValid)
			{
				SetDirtyBit(1); //force serialization
			}
		}
	}

	[Command(channel = 0)]
	public void CmdUpdateMotion(int messageId, int data)
	{
		//running on server
		if (isServer && m_sender != null)
		{
			//Debug.Log("Netstreamer: "+messageId);
			m_sender.UpdateClientMotion (messageId, data);
		}
	}
	
	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		//server side
		EncodeSerialize(writer, m_stateToSend);
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//client side
		ServerState state = SerializeDecode(reader);
		if (state.isValid && m_receiver != null)
		{
			m_receiver.ReportServerStatus(state);
		}
	}

	private void EncodeSerialize(NetworkWriter writer, ServerState state)
	{
		int id = state.messageId;
		bool inhibit = state.inhibitReconciliation;
		int input = state.inputData;

		//TODO: compress
		writer.Write (id);
		writer.Write (inhibit);
		writer.Write (input);
		PlayerProperties properties = state.properties;
		properties.NetworkSerialize(writer);
	}

	private ServerState SerializeDecode(NetworkReader reader)
	{
		int id = 0;
		int input = 0;
		bool inhibit = false;

		id = reader.ReadInt32();
		inhibit = reader.ReadBoolean();
		input = reader.ReadInt32();
		PlayerProperties properties = new PlayerProperties();
		properties.NetworkDeserialize(reader);

		ServerState state = new ServerState(id, properties, input, inhibit);

		return state;
	}
}
