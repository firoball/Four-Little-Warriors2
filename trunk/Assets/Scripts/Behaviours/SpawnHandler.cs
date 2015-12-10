using UnityEngine;
using System.Collections;

public class SpawnHandler : MonoBehaviour 
{
	private ObjectSpawnPoint spawnPoint = null;

	void Start () 
	{
	
	}

	public void SetSpawnPoint(ObjectSpawnPoint objectSpawnPoint)
	{
		spawnPoint = objectSpawnPoint;
	}

	void OnDestroy()
	{
		if (spawnPoint != null)
		{
			spawnPoint.TriggerRespawn();
		}
	}
}
