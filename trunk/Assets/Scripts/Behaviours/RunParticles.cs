using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class RunParticles : MonoBehaviour 
{
	private PlayerInput m_input;
	private PlayerMovement m_movement;
	private ParticleSystem m_particle;

	void Start () 
	{
		m_input = GetComponentInParent<PlayerInput>();
		m_movement = GetComponentInParent<PlayerMovement>();
		m_particle = GetComponent<ParticleSystem>();
	}
	
	void Update () 
	{
		InputData inputData = m_input.GetInputData();
		PlayerProperties properties = m_movement.GetProperties();
		if (inputData.run > 0.0f && properties.isGrounded)
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
