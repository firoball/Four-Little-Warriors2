using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IChatInputEventTarget : IEventSystemHandler 
{
	void OnInputStart(ChatClient client);
	void OnInputEnd(ChatClient client);
	void OnInputChange(ChatClient client);
}
