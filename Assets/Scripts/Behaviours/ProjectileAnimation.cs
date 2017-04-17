using UnityEngine;
using System.Collections;

public class ProjectileAnimation : MonoBehaviour 
{

	private bool m_timeoutTrigger;
	private Color m_color;
	private Material m_material;
	private Renderer m_renderer;
	private ParticleSystem m_particles;
	
	[Range(0.1f,10.0f)]
	public float m_timeoutSpeed = 1.0f;
	[Range(0.1f,1.0f)]
	public float m_rotateSpeed = 1.0f;
	
	public void SetTimeoutTrigger()
	{
		m_timeoutTrigger = true;
		m_material.color = m_color;
	}
	
	public void SetCollisionTrigger()
	{
		if (m_renderer == null) 
		{
			return;
		}
		m_renderer.enabled = false;
		if (m_particles != null)
		{
			m_particles.Play();
		}
	}
	
	void Start () 
	{
		//store initial setup
		m_renderer = gameObject.GetComponent<Renderer>();
		m_material = m_renderer.material;
		m_color = m_material.color;
		m_particles = gameObject.GetComponent<ParticleSystem>();

		m_timeoutTrigger = false;
	}
	
	void Update () 
	{
		transform.Rotate(Vector3.forward, m_rotateSpeed * 360 * Time.deltaTime);

		if (m_timeoutTrigger)
		{
			float alpha = m_material.color.a - m_timeoutSpeed * Time.deltaTime;
			if (alpha <= 0.0f)
			{
				alpha = 0.0f;
				m_timeoutTrigger = false;
			}
			Color color = new Color(m_material.color.r, m_material.color.g, m_material.color.b, alpha);
			m_material.color = color;
		}		
	}
}
