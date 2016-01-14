using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent (typeof(ParticleSystem))]
[RequireComponent (typeof(Collider))]
public class TrapFireBeam : ObjectEventManager 
{
	public float m_offDuration = 2.0f;
	public float m_onDuration = 5.0f;

	private ParticleSystem m_particle;
	private Collider m_collider;

	private const float c_colliderDelay = 1.0f;

	//running only on server/host
	protected override bool OnEventTrigger(Collider collider)
	{
		//do stuff?
		return true;
	}
	
	protected override void RpcOnRemoteEventTrigger()
	{
		//do stuff? spawn smoke?
	}
	
	void Awake () 
	{
		m_particle = GetComponentInChildren<ParticleSystem>();
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider coll in colliders)
		{
			if (coll.isTrigger)
			{
				m_collider = coll;
				break;
			}
		}
		Debug.Log(m_collider);
	}

	public override void OnStartServer()
	{
		StartCoroutine(ToggleEffect());
	}
	
	[ClientRpc]
	private void RpcChangeStatus(bool active)
	{
		ChangeStatus(active);
	}

	private void ChangeStatus(bool active)
	{
		if (active)
		{
			//m_collider.enabled = true;
			StartCoroutine(DelayedCollider());
			m_particle.enableEmission = true;
		}
		else
		{
			m_collider.enabled = false;
			m_particle.enableEmission = false;
		}
	}

	private IEnumerator ToggleEffect()
	{
		while(gameObject != null)
		{
			ChangeStatus(true);
			if (isServer && !isClient)
			{
				RpcChangeStatus(true);
			}
			yield return new WaitForSeconds(m_onDuration);

			ChangeStatus(false);
			if (isServer && !isClient)
			{
				RpcChangeStatus(false);
			}
			yield return new WaitForSeconds(m_offDuration);			
		}
	}

	private IEnumerator DelayedCollider()
	{
		yield return new WaitForSeconds(c_colliderDelay);			
		m_collider.enabled = true;
	}

	void OnDestroy()
	{
		StopCoroutine(ToggleEffect());
	}
}
