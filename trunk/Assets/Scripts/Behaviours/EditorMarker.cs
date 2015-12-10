using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorMarker : MonoBehaviour 
{
	public EditorMarkerType markerType = EditorMarkerType.WIRECUBE;
	public float markerSize = 1.0f;
	public float directionSize = 2.0f;

	private Collider[] colliders;
	void Start ()
	{
		colliders = GetComponents<Collider>();
	}
	
	void OnDrawGizmos()
	{
		DrawName();
		DrawMarker();
		DrawRotation();
		DrawColliders();

	}
	
	private void DrawName()
	{
		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.MiddleCenter;
		Handles.color = Color.white;
		Handles.Label(transform.position, this.name, style);
	}

	private void DrawMarker()
	{
		Gizmos.color = Color.yellow;
		Vector3 box = new Vector3(markerSize, markerSize, markerSize);

		switch (markerType)
		{
		case (EditorMarkerType.CUBE):
		{
			Gizmos.DrawCube(transform.position, box);
			break;
		}
			
		case (EditorMarkerType.WIRECUBE):
		{
			Gizmos.DrawWireCube(transform.position, box);
			break;
		}
			
		case (EditorMarkerType.SPHERE):
		{
			Gizmos.DrawSphere(transform.position, markerSize);
			break;
		}
			
		case (EditorMarkerType.WIRESPHERE):
		{
			Gizmos.DrawWireSphere(transform.position, markerSize);
			break;
		}
			
		default:
		{
			break;
		}
		}
	}

	private void DrawRotation()
	{
		Vector3 line = transform.position + transform.rotation * new Vector3(0.0f, 0.0f, directionSize);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, line);
		Gizmos.DrawSphere(line, 0.2f);
	}

	private void DrawColliders()
	{
		if (colliders != null)
		{
			foreach (Collider coll in colliders)
			{
				if (coll.enabled)
				{
					if (coll.isTrigger)
					{
						Gizmos.color = Color.magenta;
					}
					else
					{
						Gizmos.color = Color.green;
					}
					Vector3 min = coll.bounds.min;
					Vector3 max = coll.bounds.max;
					Gizmos.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z));
					Gizmos.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z));
					Gizmos.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z));
					Gizmos.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, min.y, min.z));
					
					Gizmos.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z));
					Gizmos.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z));
					Gizmos.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z));
					Gizmos.DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(min.x, min.y, max.z));
					
					Gizmos.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z));
					Gizmos.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));
					Gizmos.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z));
					Gizmos.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z));
				}
			}
		}
	}
}

public enum EditorMarkerType : int
{
	NONE = 0,
	CUBE = 1,
	WIRECUBE = 2,
	SPHERE = 3,
	WIRESPHERE = 4,
	ICON = 5
}