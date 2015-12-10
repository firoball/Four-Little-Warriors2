using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class ChatServer : MonoBehaviour
{
	public static ChatServer singleton = null;
	
	private bool m_initialized = false;
	private List<int> m_lockList;

	private const float c_lockTime = 1.0f;
	private const int c_messageMaxLength = 80;
	
	public static void Initialize()
	{
		if (singleton != null)
		{
			singleton.m_lockList = new List<int>();
			NetworkServer.RegisterHandler(ExtendedMsgType.Chat, singleton.OnSvChatMessage);
			singleton.m_initialized = true;
		}
		else
		{
			Debug.LogError("ChatServer: not instantiated. Chat will not be initialized.");
		}
	}
	
	public void SendChatMessage(string msg, ChatMode mode = ChatMode.INFO)
	{
		//only send in online mode, may be used for service messages from server
		if (m_initialized)
		{
			ChatMessage message = new ChatMessage(-1, mode, msg); //id -1 = server
			SendChatMessage(message);
		}
	}
	
	private void SendChatMessage(ChatMessage message)
	{
		//security checks
		message.content = message.content.Trim();
		if (message.content.Length > c_messageMaxLength)
		{
			message.content.Remove(c_messageMaxLength);
		}
		if (message.mode >= ChatMode.MAXVALUE)
		{
			message.mode = ChatMode.ERROR;
		}
		if (!m_lockList.Contains(message.senderId))
		{
			if (message.mode == ChatMode.TEAM)
			{
				//pick connections in same team
				List<int> teamConnectionIds = PlayerManager.GetTeamConnectionIds(message.senderId);
				foreach (int connectionId in teamConnectionIds)
				{
					NetworkServer.SendToClient(connectionId, ExtendedMsgType.Chat, message);
				}
			}
			else
			{
				//just broadcast to everyone
				NetworkServer.SendToAll(ExtendedMsgType.Chat, message);
			}
			//activate message spam lock for sender connection
			StartCoroutine("SendLock", message.senderId);
		}
	}

	private void OnSvChatMessage(NetworkMessage msg)
	{
		//only send in online mode, may be used for service messages from server
		if (m_initialized)
		{
			ChatMessage message = msg.ReadMessage<ChatMessage>();
			NetworkConnection connection = msg.conn;
			//discard original connectionId and use the one from NetworkConnection
			message.senderId = connection.connectionId;
			SendChatMessage(message);
		}
	}
	
	void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
		}
		else
		{
			Debug.LogWarning("ChatServer: Multiple instance detected. Destroying...");
			Destroy (this);
		}
	}
	
	IEnumerator SendLock(int connectionId)
	{
		m_lockList.Add(connectionId);
		yield return new WaitForSeconds(c_lockTime);
		m_lockList.Remove(connectionId);
	}
	
	public static void Lock()
	{
		if (singleton != null)
		{
			singleton.m_initialized = false;
		}
	}
}
