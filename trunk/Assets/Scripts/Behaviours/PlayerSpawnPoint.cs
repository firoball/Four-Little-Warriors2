using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawnPoint : MonoBehaviour 
{
	private int index;
	private static int iterator = 0;

	public static List<PlayerSpawnPoint> spawnPoints = new List<PlayerSpawnPoint>();

	public static PlayerSpawnPoint Next()
	{
		if (spawnPoints.Count == 0)
		{
			return null;
		}
		else
		{
			int i = iterator;
			iterator++;
			if (iterator >= spawnPoints.Count)
			{
				iterator = 0;
			}

			return spawnPoints[i];
		}
	}

	public static PlayerSpawnPoint Get(int id)
	{
		foreach(PlayerSpawnPoint spawnPoint in spawnPoints)
		{
			if (id == spawnPoint.id)
			{
				return spawnPoint;
			}
		}
		return null;
	}

	void Start () 
	{
		spawnPoints.Add(this);
		index = spawnPoints.Count - 1;
		NetworkManager.RegisterStartPosition(transform);
	}
	
	void OnDestroy()
	{
		spawnPoints.Remove(this);
	}

	public Vector3 position
	{
		get {return transform.position;}
	}

	public Quaternion rotation
	{
		get {return transform.rotation;}
	}

	public int id
	{
		get {return index;}
	}
}
