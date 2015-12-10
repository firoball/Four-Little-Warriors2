using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Text))]
public class HudChatLog : MonoBehaviour, IChatLogEventTarget 
{
	private Text m_text;

	void Start () 
	{
		m_text = GetComponent<Text>();
		ChatClient.SubscribeEvents(gameObject);
	}
	
	void OnDestroy()
	{
		ChatClient.UnSubscribeEvents(gameObject);
	}

	public void OnLogMessage(ChatClient chatClient)
	{
		//TODO: play some message reception sound maybe?
		OnLogChange(chatClient);
	}

	public void OnLogChange(ChatClient chatClient)
	{
		IList<ChatMessage> msgBuffer = chatClient.MessageBuffer;
		m_text.text = string.Empty;
		foreach (ChatMessage message in msgBuffer)
		{
			//get player names
			List<PlayerIdentity> identities = PlayerManager.GetPlayerIdentities(message.senderId);
			string names = string.Empty;
			foreach(PlayerIdentity identity in identities)
			{
				if (names != string.Empty)
					names += "/";
				names += identity.Name;
			}
			
			//mark team messages
			if (message.mode == ChatMode.TEAM)
			{
				names = "(Team) "+ names;
			}
			
			//now add message to text
			if (!string.IsNullOrEmpty(m_text.text))
			{
				m_text.text += "\n";
			}
			m_text.text += names + ": " + message.content;
		}
	}
	
}
