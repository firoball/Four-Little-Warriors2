using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class EventSubscription : MonoBehaviour
{
	private List<GameObject> m_subscribers = new List<GameObject>();

	public void SubscribeEvents(GameObject subscriber)
	{
		m_subscribers.Add(subscriber);
	}

	public void UnSubscribeEvents(GameObject subscriber)
	{
		m_subscribers.Remove(subscriber);
	}
	
	public void ExecuteEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
	{
		foreach (GameObject subscriber in m_subscribers)
		{
			//filter subscriber list by their event interface implementation
			if (subscriber.GetComponent<T>() != null)
			{
				ExecuteEvents.Execute<T>(subscriber, eventData, functor);
			}
		}
	}
}
