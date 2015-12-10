using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasCameraScale : MonoBehaviour 
{
	void Awake()
	{
		Canvas canvas = GetComponent<Canvas>();
		canvas.enabled = false;
	}

	void Start()
	{
		Canvas canvas = GetComponent<Canvas>();
		CanvasScaler scaler = GetComponent<CanvasScaler>();
		if (canvas != null && scaler != null)
		{
			canvas.enabled = true;
			Camera camera = canvas.worldCamera;
			if (camera != null)
			{
				Rect rect = camera.rect;
				Vector2 res = scaler.referenceResolution;
				//res /= rect.size;
				res.x /= rect.width;
				res.y /= rect.height;
				scaler.referenceResolution = res;
			}
		}
	}
}
