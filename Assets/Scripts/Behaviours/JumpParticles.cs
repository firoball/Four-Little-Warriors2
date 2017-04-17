using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class JumpParticles : MonoBehaviour 
{
	private PlayerMovement m_movement;
	private ParticleSystem m_particle;
	public/*private*/ bool m_inhibited = false;
	public/*private*/ bool m_lastGrounded = false;
	
	void Start () 
	{
		m_movement = GetComponentInParent<PlayerMovement>();
		m_particle = GetComponent<ParticleSystem>();
	}
	
	void Update () 
	{
		PlayerProperties properties = m_movement.GetProperties();
		if (!m_lastGrounded && properties.isGrounded && !m_inhibited)
		{
			m_particle.time = 0.0f;
			m_particle.Play();
			StartCoroutine(EffectInhibition());
		}
		m_lastGrounded = properties.isGrounded;
	}

	void OnDestroy ()
	{
		StopCoroutine(EffectInhibition());
	}

	private IEnumerator EffectInhibition()
	{
		m_inhibited = true;
		yield return new WaitForSeconds(0.4f);
		m_inhibited = false;
	}
}
