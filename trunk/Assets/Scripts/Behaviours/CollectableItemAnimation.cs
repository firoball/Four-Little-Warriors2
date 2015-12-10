using UnityEngine;
using System.Collections;

public class CollectableItemAnimation : MonoBehaviour 
{

	private bool m_spawnTrigger;
	private bool m_collectTrigger;
	private Color m_color;
	private Vector3 m_scale;
	private float m_scaleFactor;
	private Material m_material;

	[Range(0.1f,10.0f)]
	public float m_spawnSpeed = 1.0f;
	[Range(0.1f,10.0f)]
	public float m_collectSpeed = 1.0f;
	[Range(0.1f,1.0f)]
	public float m_rotateSpeed = 1.0f;

	public void SetSpawnTrigger()
	{
		m_spawnTrigger = true;
		transform.localScale = Vector3.zero;
		m_scaleFactor = 0.0f;
	}

	public void SetCollectTrigger()
	{
		m_collectTrigger = true;
		m_material.color = m_color;
	}

	void Awake () 
	{
		//store initial setup
		m_material = gameObject.GetComponent<Renderer>().material;
		m_color = m_material.color;
		m_scale = transform.localScale;

		m_spawnTrigger = false;
		m_collectTrigger = false;
	}
	
	void Update () 
	{
		transform.Rotate(Vector3.up, m_rotateSpeed * 360 * Time.deltaTime);
		//new Vector3(transform.eulerAngles.x, m_rotateSpeed * 360 * Time.deltaTime, transform.eulerAngles.z));

		if (m_collectTrigger)
		{
			float alpha = m_material.color.a - m_collectSpeed * Time.deltaTime;
			if (alpha <= 0.0f)
			{
				alpha = 0.0f;
				m_collectTrigger = false;
			}
			Color color = new Color(m_material.color.r, m_material.color.g, m_material.color.b, alpha);
			m_material.color = color;
		}

		if (m_spawnTrigger)
		{
			m_scaleFactor = m_scaleFactor + m_collectSpeed * Time.deltaTime;
			if (m_scaleFactor >= 1.0f)
			{
				m_scaleFactor = 1.0f;
				m_spawnTrigger = false;
			}
			transform.localScale = Vector3.Lerp(Vector3.zero, m_scale, m_scaleFactor);
		}	
	}
}
