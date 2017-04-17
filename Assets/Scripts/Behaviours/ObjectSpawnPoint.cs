using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ObjectSpawnPoint : MonoBehaviour 
{
	public bool enableRespawn = false;
	[Range(1.0f, 60.0f)]
	public float respawnTime = 10.0f;
	public bool dontSpawnIfPlayerNear = false;
	public float playerNearDist = 20.0f;
	[SerializeField]
	public List<SpawnableObjectProperties> items = new List<SpawnableObjectProperties>();

	private bool m_objectSpawned = false;
	private int m_totalWeight = 0;

	void Awake () 
	{
		Collider[] colliders = GetComponents<Collider>();
		Renderer render = GetComponent<Renderer>();
		
		//UnityEngine.Random.seed = 23;

		//don't bug anyone
		if (colliders != null)
		{
			foreach (Collider collider in colliders)
			{
				collider.enabled = false;
			}
		}
		if (render != null)
		{
			render.enabled = false;
		}
		//get rid of all child objects
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}
		playerNearDist = Mathf.Max(playerNearDist, 1.0f);
		foreach (SpawnableObjectProperties properties in items)
		{
			m_totalWeight += properties.weight;
		}
		//no other scripts please
		MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour behaviour in behaviours)
		{
			if (
				behaviour.GetType() != typeof(ObjectSpawnPoint) &&
				behaviour.GetType() != typeof(NetworkIdentity) &&
				behaviour.GetType() != typeof(EditorMarker)
				)
			{
				Destroy (behaviour);
			}
		}
	}

	void Start()
	{
		Activate ();
	}

	/*void OnEnable()
	{
		Debug.Log("OnEnable");
		Activate ();
	}*/

	private IEnumerator WaitRespawn()
	{
		yield return new WaitForSeconds(respawnTime);
		if (dontSpawnIfPlayerNear)
		{
			while(PlayerManager.GetNearestPlayerDist(transform.position) < playerNearDist)
			{
				yield return new WaitForSeconds(0.2f);
			}
		}
		SpawnObject();
	}

	private void Activate()
	{
		if (items.Count == 0)
		{
			Debug.LogWarning("ObjectSpawnPoint: no gameObject assigned");
			return;
		}
		SpawnObject();
	}

	private void SpawnObject()
	{
		//Debug.Log("SpawnObject");
		if (m_objectSpawned)
		{
			return;
		}

		GameObject item = null;
		if (items.Count > 1)
		{
			int rnd = Convert.ToInt32(UnityEngine.Random.value * Convert.ToSingle(m_totalWeight));
			int weightCount = 0;
			for (int i = 0; i < items.Count; i++)
			{
				weightCount += items[i].weight;
				if (rnd <= weightCount)
				{
					item = items[i].item;
					break;
				}
			}
		}
		else
		{
			item = items[0].item;
		}
		GameObject obj = (GameObject)Instantiate(item, transform.position, item.transform.rotation);
		SpawnHandler handler = obj.GetComponent<SpawnHandler>();
		if (handler != null)
		{
			handler.SetSpawnPoint(this);
		}
		else if (enableRespawn)
		{
			Debug.LogWarning("ObjectSpawnPoint: " + obj.name + " does not support respawn (SpawnHandler Component missing)");
		}
		NetworkServer.Spawn(obj);
		m_objectSpawned = true;
	}

	public void TriggerRespawn()
	{
		m_objectSpawned = false;
		if (enableRespawn && gameObject.activeSelf)
		{
			StartCoroutine(WaitRespawn());
		}
	}

}

[Serializable]
public struct SpawnableObjectProperties
{
	public GameObject item;
	[Range(1, 10)]
	public int weight;
}
