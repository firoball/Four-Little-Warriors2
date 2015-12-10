using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IStartupEventTarget : IEventSystemHandler 
{
	void OnLocalGameInitialized();
	void OnNetworkGameInitialized();
}
