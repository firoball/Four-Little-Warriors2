using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class RunParticles : MonoBehaviour 
{
	private PlayerMovement m_movement;
	private ParticleSystem m_particle;

	void Start () 
	{
		m_movement = GetComponentInParent<PlayerMovement>();
		m_particle = GetComponent<ParticleSystem>();
	}
	
	void Update () 
	{
		PlayerProperties properties = m_movement.GetProperties();
		if (properties.isRunning && properties.isGrounded)
		{
			m_particle.enableEmission = true;
		//	m_particle.Play();
		}
		else
		{
			m_particle.enableEmission = false;
		//	m_particle.Stop();
		}
	}
}
