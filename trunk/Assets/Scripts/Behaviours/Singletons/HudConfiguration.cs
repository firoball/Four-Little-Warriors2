using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class HudConfiguration : MonoBehaviour 
{
	private static HudConfiguration singleton = null;

	private List<HudProperties[]> m_viewportConfig;
	private Dictionary<GameObject, GameObject> m_views;
	private GameObject m_defaultCam;

	public GameObject playerView;
	//public HudProperties[] viewportNetwork;
	public HudProperties[] viewportSingle;
	public HudProperties[] viewportDual;
	public HudProperties[] viewportTriple;
	public HudProperties[] viewportQuad;

	void Start () 
	{
		if (singleton != null)
		{
			Debug.LogWarning("HudConfiguration: Multiple instance detected. Destroying...");
			Destroy (gameObject);
		}
		else
		{
			singleton = this;
			//keep through level change
			DontDestroyOnLoad(gameObject);

			m_viewportConfig = new List<HudProperties[]>();
			m_views = new Dictionary<GameObject, GameObject>();
			m_defaultCam = GameObject.Find("Main Camera");
			//viewportConfig.Add (viewportNetwork);
			m_viewportConfig.Add (viewportSingle);
			m_viewportConfig.Add (viewportDual);
			m_viewportConfig.Add (viewportTriple);
			m_viewportConfig.Add (viewportQuad);
		}
	}

	public static void CreateHud(GameObject player)
	{
		if (singleton != null)
		{
			singleton.SetupHud(player);
		}
		else
		{
			Debug.LogWarning("HudConfiguration: no instance in scene. Cannot create HUD.");
		}
	}

	public static void DestroyHud(GameObject player)
	{
		if (singleton != null)
		{
			singleton.RemoveHud(player);
		}
		else
		{
			Debug.LogWarning("HudConfiguration: no instance in scene. Cannot destroy HUD.");
		}
	}

	public void SetupHud(GameObject player)
	{
		if (playerView != null)
		{
			//kill main camera
			if (m_views.Count == 0)
			{
				if (m_defaultCam != null)
				{
					m_defaultCam.SetActive(false);
				}
				//Destroy(defaultCam);
			}

			GameObject view = (GameObject)GameObject.Instantiate(playerView);
			view.name = "PlayerView";
			m_views.Add(player, view);

			int id = 0;
			PlayerIdentity identity = player.GetComponent<PlayerIdentity>();
			if (identity != null)
			{
				view.name += " id: " + identity.ControllerId;
				id = Convert.ToInt32(identity.ControllerId);
			}

			HudProperties[] properties = m_viewportConfig[Settings.localPlayers - 1];
			int index = Math.Min(Math.Max(id, 0), Settings.localPlayers - 1);

			//set viewport size for any camera found in playerView children
			Camera[] cameras = view.GetComponentsInChildren<Camera>();
			foreach (Camera camera in cameras)
			{
				SetupCamera (camera, properties[index]);
			}

			//now attach everything to player object
			ExecuteEvents.Execute<IAttachableEventTarget>(view, null,(x,y)=>x.OnAttach(player));
		}
	}

	public void RemoveHud(GameObject player)
	{
		GameObject view;
		if(m_views.TryGetValue(player, out view))
		{
			m_views.Remove(player);
			//now detach everything from player object
			ExecuteEvents.Execute<IAttachableEventTarget>(view, null,(x,y)=>x.OnDetach(player));
		}

		//all views detached? restore main camera
		if (m_views.Count == 0)
		{
			if (m_defaultCam != null)
			{
				m_defaultCam.SetActive(true);
			}
		}
	}

	private void SetupCamera(Camera camera, HudProperties properties)
	{
		camera.rect = properties.viewport;
		//extend
	}

}

[Serializable]
public class HudProperties
{
	public Rect viewport = new Rect(0, 0, 1, 1);
	public Vector2 timePanelPos;
	public Rect chatLogPos;
	public Rect chatInputPos;
	//extend
}
