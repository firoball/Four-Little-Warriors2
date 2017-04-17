using UnityEngine;
using System.Collections;
using System;

public class CollectableItem : ObjectEventManager 
{
	public CollectableProperties properties;
	//public float destroyDelay = 1.0f;
	private Animator animator;
	private CollectableItemAnimation itemAnimation;

	void Start () 
	{
		itemAnimation = GetComponent<CollectableItemAnimation>();
		animator = GetComponent<Animator>();
		if (itemAnimation)
		{
			itemAnimation.SetSpawnTrigger();
		}
		else if (animator)
		{
			animator.SetTrigger("spawnTrigger");
		}
	}
	
	//running only on server/host
	protected override bool OnEventTrigger(Collider collider)
	{
		Debug.Log("Host Collect");
		GameObject collector = collider.gameObject;
		CollectableItemContainer container = collector.GetComponent<CollectableItemContainer>();
		if (container != null)
		{
			if (container.Collect(properties))
			{
				//StartCoroutine(WaitDestroy());
				return true;
			}
		}

		return false;
	}

	protected override void RpcOnRemoteEventTrigger()
	{
		Debug.Log("Remote Collect");
		if (itemAnimation)
		{
			itemAnimation.SetCollectTrigger();
		}
		else if (animator)
		{
			animator.SetTrigger("collectTrigger");
		}
	}

/*	private IEnumerator WaitDestroy()
	{
		yield return new WaitForSeconds(destroyDelay);
		Destroy();
	}*/
}
