using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerView : MonoBehaviour, IAttachableEventTarget, IAttributeEventTarget 
{
	private GameObject m_target = null;
	private HudAttributeDisplay[] m_hudAttributeDisplays = null;

	public GameObject Target 
	{
		get {return m_target;}
	}

	void Awake()
	{
		m_hudAttributeDisplays = GetComponentsInChildren<HudAttributeDisplay>();
	}

	public void OnAttach (GameObject target)
	{
		if (target != null)
		{
			m_target = target;
		}
		//Debug.Log("PlayerView OnAttach");
		foreach (Transform child in transform)
		{
			ExecuteEvents.Execute<IAttachableEventTarget>(child.gameObject, null,(x,y)=>x.OnAttach(target));
		}
		PlayerAttributes playerAttributes = target.GetComponent<PlayerAttributes>();
		if (playerAttributes != null)
		{
			playerAttributes.SubscribeEvents(gameObject);
		}
	}

	public void OnDetach (GameObject target)
	{
		//Debug.Log("PlayerView: OnDetach");
		foreach (Transform child in transform)
		{
			ExecuteEvents.Execute<IAttachableEventTarget>(child.gameObject, null,(x,y)=>x.OnDetach(target));
		}
		PlayerAttributes playerAttributes = target.GetComponent<PlayerAttributes>();
		if (playerAttributes != null)
		{
			playerAttributes.UnSubscribeEvents(gameObject);
		}
		Destroy(gameObject);
	}

	public void OnAttributeChange(PlayerAttribute attribute)
	{
		if (m_hudAttributeDisplays == null)
		{
			return;
		}

		foreach(HudAttributeDisplay hudAttributeDisplay in m_hudAttributeDisplays)
		{
			if (hudAttributeDisplay.valueType == attribute.type)
			{
				ExecuteEvents.Execute<IAttributeEventTarget>(hudAttributeDisplay.gameObject, null,(x,y)=>x.OnAttributeChange(attribute));
			}
		}
	}

}
