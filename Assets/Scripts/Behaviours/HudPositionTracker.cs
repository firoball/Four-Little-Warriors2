using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(RectTransform))]
public class HudPositionTracker : MonoBehaviour
{
	public GameObject m_trackerImage = null;
	public Camera m_camera = null;
	public float m_nearDistanceView = 0.7f;
	public float m_distanceScale = 2.0f;
	[Range(0.1f,1.0f)]
	public float m_minDistanceView = 0.25f;
	[Range(0.1f,1.0f)]
	public float m_maxDistanceView = 0.6f;
	[Range(0.1f,1.0f)]
	public float m_minScaleTracker = 0.2f;
	[Range(0.1f,1.0f)]
	public float m_maxScaleTracker = 1.0f;
	[Range(0.1f,1.0f)]
	public float m_fadeSpeed = 0.5f;

	private List<RectTransform> m_rects;
	private List<Image> m_images;
	private List<PlayerIdentity> m_playerIdentities;
	private GameObject m_player;
	private RectTransform m_rect;
	private PlayerView m_playerView;

	void Start () 
	{
		m_rects = new List<RectTransform>();
		m_images = new List<Image>();
		m_playerIdentities = PlayerManager.Players;

		//get target (player) from root PlayerView
		Transform parent = transform.parent;
		m_playerView = parent.GetComponentInParent<PlayerView>();
		m_rect = parent.GetComponent<RectTransform>();
	}
	
	void Update () 
	{
		m_player = m_playerView.Target;
		if (m_player != null)
		{
			UpdateImageInstances(m_playerIdentities.Count);
			UpdateImagePositions(m_playerIdentities);
		}
	}

	private void UpdateImageInstances(int count)
	{
		count--; //own player does not need tracker image
		if (count < 0 || count == m_rects.Count)
		{
			return;
		}

		while (count < m_rects.Count)
		{
			GameObject objImage = m_rects[0].gameObject;
			m_rects.RemoveAt(0);
			m_images.RemoveAt(0);
			Destroy(objImage);
		}

		if (m_trackerImage != null)
		{
			while (count > m_rects.Count)
			{
				GameObject objImage = (GameObject)Instantiate (m_trackerImage);
				objImage.transform.SetParent(transform, false);
				RectTransform rect = objImage.GetComponent<RectTransform>();
				Image image = objImage.GetComponent<Image>();
				if (rect != null && image != null)
				{
					//Debug.Log("added image");
					m_rects.Add(rect);
					m_images.Add(image);
				}
			}
		}
	}

	private void UpdateImagePositions(List<PlayerIdentity> playerIdentities)
	{
		int index = 0;
		foreach (PlayerIdentity playerIdentity in playerIdentities)
		{
			GameObject foreignPlayer = playerIdentity.gameObject;
			if (foreignPlayer == m_player)
			{
				continue;
			}

			//view space calculations
			Vector3 vecHudPos = m_camera.WorldToViewportPoint(foreignPlayer.transform.position);
			Vector3 vecHudOrigin = m_camera.WorldToViewportPoint(m_player.transform.position);
			vecHudPos -= vecHudOrigin;
			vecHudPos.z = 0.0f;
			if (vecHudPos.sqrMagnitude > m_nearDistanceView * m_nearDistanceView)
			{
				vecHudPos -= Vector3.Normalize(vecHudPos) * m_nearDistanceView;
				//image.gameObject.SetActive(true);
				FadeImage(index, 1.0f);
			}
			else
			{
				vecHudPos = Vector3.Normalize(vecHudPos) * 0.001f;
				//image.gameObject.SetActive(false);
				FadeImage(index, 0.0f);
			}
			vecHudPos /= m_distanceScale;

			//apply view space limits
			if (vecHudPos.sqrMagnitude > m_maxDistanceView * m_maxDistanceView)
			{
				vecHudPos = Vector3.Normalize(vecHudPos) * m_maxDistanceView;
			}
			else if (vecHudPos.sqrMagnitude < m_minDistanceView * m_minDistanceView)
			{
				vecHudPos = Vector3.Normalize(vecHudPos) * m_minDistanceView;
			}

			TransformImage(index, vecHudPos);

			index++;
		}
	}

	private void FadeImage(int index, float targetAlpha)
	{
		Image image = m_images[index];
		float alpha = image.color.a;
		if (alpha < targetAlpha)
		{
			alpha += m_fadeSpeed * Time.deltaTime;
			alpha = Mathf.Min(alpha, targetAlpha);
			Color color = new Color(image.color.r, image.color.g, image.color.b, alpha);
			image.color = color;
		}
		else if (alpha > targetAlpha)
		{
			alpha -= m_fadeSpeed * Time.deltaTime;
			alpha = Mathf.Max(alpha, targetAlpha);
			Color color = new Color(image.color.r, image.color.g, image.color.b, alpha);
			image.color = color;
		}
	}

	private void TransformImage(int index, Vector3 pos)
	{
		RectTransform rect = m_rects[index];

		//scale
		float scale =  1.0f - ((pos.magnitude - m_minDistanceView) / (m_maxDistanceView - m_minDistanceView));
		scale = Mathf.Max(scale * m_maxScaleTracker, m_minScaleTracker);
		rect.localScale = Vector3.Lerp(new Vector3(scale, scale, 1.0f), rect.localScale, 0.5f);
		
		//position
		Vector2 vecScreenSize = m_rect.sizeDelta;
		pos.Scale(new Vector3(vecScreenSize.x * 0.5f, vecScreenSize.y * 0.5f, 0.0f));
		rect.localPosition = Vector3.Lerp(rect.localPosition, pos, 0.3f);
		
		//rotation
		float angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
		Quaternion hudRotation = new Quaternion();
		hudRotation.eulerAngles = new Vector3(0.0f, 0.0f, angle);//Vector3.Lerp(image.localRotation.eulerAngles, new Vector3(0.0f, 0.0f, angle), 0.7f);
		rect.localRotation = hudRotation;
	}
}
