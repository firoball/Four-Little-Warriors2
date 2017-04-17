using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IAttachableEventTarget : IEventSystemHandler
{
	void OnAttach(GameObject target);
	void OnDetach(GameObject target);
}


