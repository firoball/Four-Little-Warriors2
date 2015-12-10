using UnityEngine;
using System.Collections;

public struct ServerState 
{
	private const int invalidMessageId = 0;

	public int messageId;
	public PlayerProperties properties;
	public int inputData;

	public ServerState(int id, PlayerProperties plProperties, int input)
	{
		messageId = id;
		properties = plProperties;
		inputData = input;
	}

	public void Invalidate()
	{
		messageId = invalidMessageId;
	}

	public bool isInvalid
	{
		get {return (messageId == invalidMessageId);}
	}

	public bool isValid
	{
		get {return (messageId != invalidMessageId);}
	}
}