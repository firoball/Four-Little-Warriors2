using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IChatLogEventTarget : IEventSystemHandler 
{
	void OnLogChange(ChatClient client);
	void OnLogMessage(ChatClient client);
}
