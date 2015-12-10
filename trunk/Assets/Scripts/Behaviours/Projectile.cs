using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(DamageDealer))]
public class Projectile : ObjectEventManager
{
	[SerializeField]
	private float m_attackRange = 10.0f;
	[SerializeField]
	private float m_speed = 1.0f;
	[SerializeField]
	private Material[] m_materials;

	[ShowOnly][SerializeField][SyncVar]
	private int m_connectionId;
	[ShowOnly][SerializeField][SyncVar]
	private short m_controllerId;
	[SyncVar]
	private int m_level = 0;
	
	private DamageDealer m_damageDealer;
	private ProjectileAnimation m_projectileAnimation;
	private float m_lifeTime;
	private bool m_disableUpdate = false;
	private GameObject m_launcher;

	public GameObject Launcher 
	{
		get {return m_launcher;}
	}

	void Awake () 
	{
		m_damageDealer = GetComponent<DamageDealer>();
		m_projectileAnimation = GetComponent<ProjectileAnimation>();
		m_speed = Mathf.Max (m_speed, 0.01f);
		m_lifeTime = m_attackRange / m_speed;
		if (m_materials.Length > 0)
		{
			int index = Math.Min(m_level, m_materials.Length);
			Renderer renderer = GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material = m_materials[index];
			}
		}

	}

	void Update () 
	{
		if (isClient || !isServer)
		{
			Move (Time.deltaTime);
		}
	}

	void FixedUpdate()
	{
		//update server side only in fixed steps for lag compensation
		if (isClient || !isServer)
		{
			return;
		}

		//Lag compensation only works with connection info of player object
		if (m_launcher != null && LagCompensator.IsActive)
		{
			NetworkIdentity identity = m_launcher.GetComponent<NetworkIdentity>();
			if (identity != null)
			{
				LagCompensator.Rewind(gameObject, identity.connectionToClient);
			}
		}
		Move (Time.fixedDeltaTime);
		LagCompensator.Register(gameObject);
		LagCompensator.Restore(gameObject);
	}

	protected override bool OnEventTrigger (Collider collider)
	{
		if (collider != null)
		{
			//ignore shooting player
			if (collider.gameObject == m_launcher)
			{
				return false;
			}
			//ignore other shots from shooting player
			Projectile projectile = collider.GetComponent<Projectile>();
			if (projectile != null && projectile.Launcher == m_launcher)
			{
				return false;
			}
			else
			{
				//Debug.Log("I collided");
				if (!isServer)
				{
					TriggerCollision();
				}
				m_disableUpdate = true;
				return true;
			}
		}
		else
		{
			return false;
		}
	}

	protected override void RpcOnRemoteEventTrigger ()
	{
		TriggerCollision();
		//Debug.Log("I collided - client info");
	}

	public override void OnStartClient ()
	{
		//owner spawns own projectile in order to fit to predicted movement, so disable server controlled projectile
		if (!isServer && PlayerManager.IsLocalPlayer(m_connectionId, m_controllerId))
		{
			m_disableUpdate = true;
			name += " (deactivated)";
			gameObject.SetActive(false);
		}
		allowClientEvents = false; //server spawned object. never react on client side events
	}

	void OnDestroy()
	{
		//at this stage isServer is not reliable anymore - just Unregister, LagCompensator will catch it.
		LagCompensator.Unregister(gameObject);
	}

	public void Setup(byte damage, float range, GameObject source, int level)
	{
		m_damageDealer.Damage = damage;
		m_attackRange = Mathf.Max(range, 0.0f);
		m_lifeTime = m_attackRange / m_speed;
		if (source != null)
		{
			m_launcher = source;
			PlayerIdentity identity = source.GetComponent<PlayerIdentity>();
			if (identity != null)
			{
				m_connectionId = identity.ConnectionId;
				m_controllerId = identity.ControllerId;
			}
		}
		m_level = level;
	}

	private void TriggerCollision()
	{
		if (m_projectileAnimation != null)
		{
			m_projectileAnimation.SetCollisionTrigger();
		}
		m_disableUpdate = true;
	}

	private void TriggerTimeout()
	{
		if (m_projectileAnimation != null)
		{
			m_projectileAnimation.SetTimeoutTrigger();
		}
		m_disableUpdate = true;
	}

	private void Move(float deltaTime)
	{
		if (!m_disableUpdate)
		{
			if (m_lifeTime > 0.0f)
			{
				transform.Translate(Vector3.forward * m_speed * deltaTime); 
				m_lifeTime -= deltaTime;
			}
			else
			{
				if (isServer || allowClientEvents)
				{
					Lock ();
					//Debug.Log("I timed out");
					Destroy();
				}
				TriggerTimeout();
			}
		}
	}

	public static bool IsOwner(GameObject owner, GameObject projectile)
	{
		Projectile prj = projectile.GetComponent<Projectile>();
		if (prj != null && prj.m_launcher == owner)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

}
