using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class RunParticles : MonoBehaviour
{
    private PlayerMovement m_movement;
    private ParticleSystem m_particle;
    private ParticleSystem.EmissionModule m_emission;

    void Start()
    {
        m_movement = GetComponentInParent<PlayerMovement>();
        m_particle = GetComponent<ParticleSystem>();
        m_emission = m_particle.emission;
    }

    void Update()
    {
        PlayerProperties properties = m_movement.GetProperties();
        if (properties.isRunning && properties.isGrounded)
        {
            //m_particle.enableEmission = true;
            m_emission.enabled = true;
            //	m_particle.Play();
        }
        else
        {
            //m_particle.enableEmission = false;
            m_emission.enabled = true;
            //	m_particle.Stop();
        }
    }
}
