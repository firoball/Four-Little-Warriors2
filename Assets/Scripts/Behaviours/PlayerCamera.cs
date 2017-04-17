using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour, IAttachableEventTarget 
{
	public GameObject player = null;
	public Vector3 cameraCenter;
	public bool fixedViewAngle = false;
	public float smoothViewMove = 0.95f;

	private bool m_hasTarget= false;

	void Awake () 
	{
		//Camera camera = GetComponent<Camera>();
		if (GetComponent<Camera>() != null)
		{
			if (GetComponent<Camera>().orthographic)
			{
				//calculate look-at-center transform out of height and tilt for camera
				float tilt = transform.eulerAngles.x;
				float height = transform.position.y;
				float dist = height / Mathf.Tan((tilt * Mathf.PI) / 180.0f);
				float xzDist = -dist / Mathf.Sqrt(2); //for yaw = 45 deg.
				cameraCenter = new Vector3(xzDist, height, xzDist);
			}
			transform.position = cameraCenter;
		}
		if (!fixedViewAngle)
		{
			transform.parent = player.transform;
		}

	}
	
	void Update () 
	{
		//TODO: update display, camera and so on
		if (m_hasTarget)
		{
			if (fixedViewAngle)
			{
				Vector3 position = GetCenterPos(player);

				transform.position = Vector3.Lerp(
					transform.position, 
						cameraCenter + position, 
					smoothViewMove
					);
			}
		}
	}

	public void OnAttach(GameObject target)
	{
		if (target != null)
		{
			player = target;
			m_hasTarget = true;
			transform.position = GetCenterPos(player);
		}
	}

	public void OnDetach(GameObject target)
	{
		if (m_hasTarget && target == player)
		{
			m_hasTarget = false;
			player = null;
		}
	}
	
	private Vector3 GetCenterPos(GameObject obj)
	{
		Vector3 position;
		if (obj.GetComponent<Collider>() != null)
		{
			position = obj.GetComponent<Collider>().bounds.center;
		}
		else
		{
			position = obj.transform.position;
		}
		return position;
	}
}
