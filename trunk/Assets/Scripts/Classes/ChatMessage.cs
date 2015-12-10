using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ChatMessage : MessageBase
{
	public int senderId;
	public ChatMode mode;
	public string content;

	public ChatMessage()
	{
		this.senderId = 0;
		this.mode = ChatMode.ERROR;
		this.content = "undefined message content";
	}

	public ChatMessage(int senderId, ChatMode mode, string content)
	{
		this.senderId = senderId;
		this.mode = mode;
		this.content = content;
	}
}
