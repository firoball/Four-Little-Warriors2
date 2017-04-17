using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public abstract class CollectableItemContainer : NetworkBehaviour 
{

	public List<CollectableProperties> collectedItems = new List<CollectableProperties>();

	protected virtual bool OnCollect(CollectableProperties properties)
	{
		Debug.Log(this.name + ": OnCollect not implemented.");
		return true;
	}

	protected virtual bool OnDrop(CollectableProperties properties)
	{
		Debug.Log(this.name + ": OnDrop not implemented.");
		return true;
	}
	
	public bool Collect(CollectableProperties properties)
	{
		if (OnCollect(properties))
		{
			if (!properties.singleUsage)
			{
				collectedItems.Add(properties);
			}
			return true;
		}

		return false;
	}

	public bool Drop(CollectableProperties properties)
	{
		if (OnDrop(properties))
		{
			if(collectedItems.Remove(properties))
			{
				return true;
			}
		}
	
		return false;
	}

}
