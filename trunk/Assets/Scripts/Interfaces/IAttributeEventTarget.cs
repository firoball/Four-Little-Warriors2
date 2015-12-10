using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IAttributeEventTarget : IEventSystemHandler 
{
	void OnAttributeChange(PlayerAttribute attribute);
}