using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IObjectEventTarget : IEventSystemHandler
{
	void OnRaycastShot(Collider collider);
	void OnRaycastUse(Collider collider);
}
