using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof(EventSubscription))]
public class ChatClient : MonoBehaviour
{
	public static ChatClient singleton = null;

	[SerializeField]
	private int m_historyLength = 10;
	[SerializeField]
	private float m_messageLifetime = 5.0f; 

	private SortedList<float, ChatMessage> m_messageBuffer;
	private bool m_inputActive = false;
	private bool m_initialized = false;
	private ChatMode m_messageMode = ChatMode.ALL;
	private string m_messageInput = string.Empty;
	private bool m_sendReady = true;
	private EventSubscription m_eventSubscription = null;

	private const float c_lockTime = 1.0f;
	private const int c_messageMaxLength = 80;

	public IList<ChatMessage> MessageBuffer 
	{
		get {return m_messageBuffer.Values;}
	}

	public string MessageInput 
	{
		get {return m_messageInput;}
	}

	public ChatMode MessageMode 
	{
		get {return m_messageMode;}
	}


	void Awake()
	{
		if (singleton == null)
		{
			m_messageBuffer = new SortedList<float, ChatMessage>();
			m_eventSubscription = GetComponent<EventSubscription>();
			singleton = this;
		}
		else
		{
			Debug.LogWarning("ChatClient: Multiple instance detected. Destroying...");
			Destroy (this);
		}
	}
	
	void Update()
	{
		if (!m_initialized)
		{
			return;
		}
		
		//TODO: migrate to PlayerInput?
		//		if (!m_inputActive && Input.GetButtonUp("Chat"))
		if (!m_inputActive && Input.GetButtonDown("Submit"))
		{
			m_messageMode = ChatMode.ALL;
			PrepareInput();
		}
		else if (!m_inputActive && Input.GetButtonDown("Team Chat"))
		{
			m_messageMode = ChatMode.TEAM;
			PrepareInput();
		}
		else if (m_inputActive && Input.GetButtonUp("Cancel"))
		{
			TriggerInputEnd();
			m_inputActive = false;
		}
		else if (m_inputActive)
		{
			m_inputActive = InputChatMessage(Input.inputString);
		}
		else
		{
			//no action
		}

		//update message log
		ProcessMessageBuffer();
	}
	
	private void PrepareInput()
	{
		m_messageInput = string.Empty;			
		if (Input.inputString.Length > 0)
		{
			Input.inputString.Remove(0);
		}
		m_inputActive = true;
		TriggerInputStart();
	}
	
	private bool InputChatMessage(string input)
	{
		bool ret = true;
		
		foreach (char c in input) 
		{
			if (ret)
			{
				//evaluate 'backspace'
				if (c == '\b')
				{
					if (m_messageInput.Length > 0)
					{
						m_messageInput = m_messageInput.Remove(m_messageInput.Length - 1);
						TriggerInputChange();
					}
				}
				//evaluate 'enter'
				else if (c == '\n' || c == '\r')
				{
					if (m_messageInput.Length > 0)
					{
						SendChatMessage(m_messageInput, m_messageMode);
					}
					TriggerInputEnd();
					ret = false;
				}
				//append letter
				else if (m_messageInput.Length < c_messageMaxLength)
				{
					m_messageInput += c;
					TriggerInputChange();
				}
			}
		}
		
		return ret;
	}
	
	public void SendChatMessage(string msg, ChatMode mode = ChatMode.ALL)
	{
		//only send in online mode
		if (m_initialized)
		{
			msg = msg.Trim();
			if (msg.Length > c_messageMaxLength)
			{
				msg.Remove(c_messageMaxLength);
			}
			if (m_sendReady)
			{
				NetworkClient client = NetManager.singleton.client;
				ChatMessage message = new ChatMessage(0, mode, msg);
				client.Send(ExtendedMsgType.Chat, message);
				
				StartCoroutine("SendLock");
			}
		}
	}
	
	public static void Initialize()
	{
		if (singleton != null)
		{
			NetworkClient client = NetManager.singleton.client;
			client.RegisterHandler(ExtendedMsgType.Chat, singleton.OnClChatMessage);
			singleton.m_initialized = true;
		}
		else
		{
			Debug.LogError("ChatClient: not instantiated. Chat will not be initialized.");
		}
	}
	
	private void OnClChatMessage(NetworkMessage msg)
	{
		ChatMessage message = msg.ReadMessage<ChatMessage>();
		if(m_messageBuffer.Count >= m_historyLength)
		{
			m_messageBuffer.RemoveAt(0);
		}
		m_messageBuffer.Add(Time.time, message);
		TriggerLogMessage();
	}
	
	private void ProcessMessageBuffer()
	{
		//remove all messages older than chatMessageLifetime
		IList<float> keys = m_messageBuffer.Keys;
		bool changed = false;
		
		if (keys.Count == 0)
		{
			return;
		}
		
		for (int i = keys.Count - 1; i >= 0; i--)
		{
			if (keys[i] + m_messageLifetime < Time.time)
			{
				m_messageBuffer.Remove(keys[i]);
				changed = true;
			}
		}

		if (changed)
		{
			TriggerLogChange();
		}
	}
	
	IEnumerator SendLock()
	{
		m_sendReady = false;
		yield return new WaitForSeconds(c_lockTime);
		m_sendReady = true;
	}
	
	public static void Lock()
	{
		if (singleton != null)
		{
			singleton.m_initialized = false;
		}
	}

	public static void SubscribeEvents(GameObject subscriber)
	{
		if (singleton != null)
		{
			singleton.m_eventSubscription.SubscribeEvents(subscriber);
		}
	}

	public static void UnSubscribeEvents(GameObject subscriber)
	{
		if (singleton != null)
		{
			singleton.m_eventSubscription.UnSubscribeEvents(subscriber);
		}
	}
	
	private void TriggerInputStart()
	{
		m_eventSubscription.ExecuteEvent<IChatInputEventTarget>(null,(x,y)=>x.OnInputStart(singleton));
	}

	private void TriggerInputEnd()
	{
		m_eventSubscription.ExecuteEvent<IChatInputEventTarget>(null,(x,y)=>x.OnInputEnd(singleton));
	}
	
	private void TriggerInputChange()
	{
		m_eventSubscription.ExecuteEvent<IChatInputEventTarget>(null,(x,y)=>x.OnInputChange(singleton));
	}
	
	private void TriggerLogChange()
	{
		m_eventSubscription.ExecuteEvent<IChatLogEventTarget>(null,(x,y)=>x.OnLogChange(singleton));
	}
	
	private void TriggerLogMessage()
	{
		m_eventSubscription.ExecuteEvent<IChatLogEventTarget>(null,(x,y)=>x.OnLogMessage(singleton));
	}
	
}
