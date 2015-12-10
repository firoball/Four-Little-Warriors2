using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class LocalManager : MonoBehaviour 
{
	public GameObject objPlayer;

	void Start () 
	{
	}

	void OnStartClient()
	{
		SpawnPlayers();
	}

	private void SpawnPlayers()
	{
		if (objPlayer != null)
		{
			for (int i = 0; i < Settings.maxPlayers; i++)
			{
				ClientScene.AddPlayer(Convert.ToInt16(i+1));
/*				PlayerSpawnPoint spawnPoint = PlayerSpawnPoint.Next();
				Vector3 position = Vector3.zero;
				Quaternion rotation = Quaternion.identity;
				if (spawnPoint != null)
				{
					position = spawnPoint.position;
					rotation = spawnPoint.rotation;
				}
				GameObject player = (GameObject)GameObject.Instantiate(objPlayer, position, rotation);
				//player.SendMessage("InitializeId", i+1);
				PlayerStats stats = player.GetComponent<PlayerStats>();
				if (stats != null)
				{
					//TODO: revise whole logic of LocalManager
					stats.RpcInitializeId(i+1);
				}
*/
			}
		}

	}
}
