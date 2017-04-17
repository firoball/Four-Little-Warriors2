using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IPushEventTarget : IEventSystemHandler
{
	void OnPush(Vector3 direction, Vector3 position, float duration, float inhibition);
}
