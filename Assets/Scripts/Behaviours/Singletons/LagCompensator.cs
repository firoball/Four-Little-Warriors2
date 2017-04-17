using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class LagCompensator : MonoBehaviour
{
	public string content;

	public bool isActive = true;
	public static LagCompensator singleton = null;

	private static Dictionary<GameObject, List<Vector3>> m_history;

	private const int c_historyLength = 100;
	private const int c_clientInterpolationDelay = 2; //client needs 3 data frames for interpolation = 2 frames delay

	public static bool IsActive 
	{
		get 
		{
			if (singleton != null)
			{
				return singleton.isActive;
			}
			else
			{
				return false;
			}
		}
		set 
		{
			if (singleton != null)
			{
				singleton.isActive = value;
			}
		}
	}

	void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			m_history = new Dictionary<GameObject, List<Vector3>>();
		}
		else
		{
			Debug.LogWarning("LagCompensator: Multiple instance detected. Destroying...");
			Destroy (this);
		}
	}

	/*void FixedUpdate()
	{
		content = "";
		foreach (KeyValuePair<GameObject, List<Vector3>> pair in m_history)
		{
			content += "["+pair.Key.name+": "+pair.Value.Count+"]";
		}
		if (!string.IsNullOrEmpty(content))
		{
			content = m_history.Count+"\n"+content;
			Debug.Log (content);
		}
	}*/

	public static void Rewind(GameObject sender, GameObject player/*, NetworkConnection connectionToClient*/)
	{
		if (!IsActive || sender == null)// || connectionToClient == null)
		{
			return;
		}
		float delay = PlayerManager.GetLatency(player);
		//byte error = 0;

		/*int rtt = NetworkTransport.GetCurrentRtt(
			connectionToClient.hostId, 
			connectionToClient.connectionId, 
			out error
		);*/
		//if (error == 0)
		//{
		//	float delay = Convert.ToSingle(rtt) * 0.001f;// * 0.5f;
			int rttDelta = Convert.ToInt32(delay / Time.fixedDeltaTime);
			int delta = Math.Max(0, rttDelta + c_clientInterpolationDelay);
			Debug.Log("LagCompensator: Rewind " + delta + " steps");
			foreach (KeyValuePair<GameObject, List<Vector3>> pair in m_history)
			{
				GameObject target = pair.Key;
				if (target != sender)
				{
					int index = pair.Value.Count - 1 - delta;
					if (index >= 0)
					{
						target.transform.position = pair.Value[index];
					}
				}
			}
		//}
	}

	public static void Restore(GameObject sender)
	{
		if (!IsActive || sender == null)
		{
			return;
		}
		Debug.Log("LagCompensator: Restore");
		foreach (KeyValuePair<GameObject, List<Vector3>> pair in m_history)
		{
			GameObject target = pair.Key;
			if (target != sender)
			{
				target.transform.position = pair.Value[pair.Value.Count - 1];
			}
		}
	}

	public static void Register(GameObject handle)
	{
		if (!IsActive || handle == null)
		{
			return;
		}

		List<Vector3> positions;
		//add position data to existing list of gameObject
		if (m_history.TryGetValue(handle, out positions))
		{
			while (positions.Count >= c_historyLength)
			{
				positions.RemoveAt(0);
			}
			positions.Add(handle.transform.position);
		}
		else //create new entry for gameObject
		{
			positions = new List<Vector3>();
			positions.Add(handle.transform.position);
			m_history.Add(handle, positions);
		}
	}

	public static void Unregister(GameObject handle)
	{
		if (handle == null)
		{
			return;
		}

		List<Vector3> positions;
		if (m_history.TryGetValue(handle, out positions))
		{
			positions.Clear();
			m_history.Remove(handle);
		}
		else
		{
			//Warning would always show up on client
			//Debug.LogWarning("LagCompensator: Cannot unregister GameObject. Was not registered...");
		}
	}
}
