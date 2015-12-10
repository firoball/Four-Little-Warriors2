using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof(Text))]
public class HudChatMessage : MonoBehaviour, IChatInputEventTarget 
{
	private Text m_text;
	private string m_marker;
	private string m_prefix;
	private bool m_active;
	[SerializeField]
	private float m_CursorBlinkTime = 0.4f;

	void Start () 
	{
		m_text = GetComponent<Text>();
		m_marker = string.Empty;
		m_prefix = string.Empty;
		m_active = false;
		ChatClient.SubscribeEvents(gameObject);
	}

	void OnDestroy()
	{
		ChatClient.UnSubscribeEvents(gameObject);
	}

	private IEnumerator ToggleMarker(ChatClient chatClient)
	{
		//Blinking cursor
		while (m_active)
		{
			if (string.IsNullOrEmpty(m_marker))
			{
				m_marker = "_";
			}
			else
			{
				m_marker = string.Empty;
			}
			OnInputChange(chatClient); //force update
			yield return new WaitForSeconds(m_CursorBlinkTime);
		}
	}

	public void OnInputStart(ChatClient chatClient)
	{
		if (chatClient.MessageMode == ChatMode.TEAM)
		{
			m_prefix = "Team Message: ";
		}
		else
		{
			m_prefix = "Message: ";
		}
		
		m_active = true;
		StartCoroutine("ToggleMarker", chatClient);
	}

	public void OnInputEnd(ChatClient chatClient)
	{
		m_text.text = string.Empty;
		m_active = false;
	}

	public void OnInputChange(ChatClient chatClient)
	{
		m_text.text = m_prefix + chatClient.MessageInput + m_marker;
	}
}
