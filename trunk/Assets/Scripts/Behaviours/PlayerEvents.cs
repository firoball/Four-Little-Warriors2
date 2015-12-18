using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent (typeof(PlayerAttributes))]
public class PlayerEvents : ObjectEventManager 
{
	[SerializeField]
	private Attributes m_lifeAttribute = Attributes.LIFE;
	PlayerAttributes attributes;

	void Start () 
	{
		attributes = GetComponent<PlayerAttributes>();
	}

	//running only on server/host
	protected override bool OnEventTrigger(Collider collider)
	{
		return InflictDamage(collider);
	}
	
	protected override bool OnEventShot(Collider collider)
	{
		return InflictDamage(collider);
	}
	
	/*protected override void RpcOnRemoteEventTrigger()
	{
		Debug.Log("PLAYER Remote Trigger");
	}
	
	protected override void RpcOnRemoteEventShot()
	{
		Debug.Log("PLAYER Remote Shot");
	}*/
	
	[ClientRpc]
	private void RpcOnPushEvent(Vector3 direction, Vector3 position, float duration)
	{
		PushEvent(direction, position, duration);
	}
	
	private void PushEvent(Vector3 direction, Vector3 position, float duration)
	{
		Debug.Log("PlayerEvents: PushEvent "+direction);
		PlayerMovement movement = GetComponent<PlayerMovement>();
		movement.OnPush(direction, position, duration); //temp, build player event system
	}

	private bool InflictDamage(Collider collider)
	{
		GameObject target = collider.gameObject;

		//somewhat clumsy owner check
		if (Projectile.IsOwner(gameObject, target))
		{
			return false;
		}
		//Debug.Log(target.name);
		DamageDealer damageDealer = target.GetComponent<DamageDealer>();
		if (damageDealer != null)
		{
			Debug.Log("PLAYER Host hit");
			/*byte value = */attributes.SubtractValue(m_lifeAttribute, damageDealer.Damage);

			if (damageDealer.Knockback > 0.0f)
			{
				Vector3 targetPos = collider.ClosestPointOnBounds(transform.position);
				Vector3 direction = transform.position - targetPos;
				direction = Vector3.Normalize(direction) * damageDealer.Knockback;
				if (isServer && !isClient)
				{
					RpcOnPushEvent(direction, transform.position, 0.5f); //only send RPC if not in host mode
				}
				PushEvent(direction, transform.position,0.5f); //update server

				//DEBUG
				contactpos = targetPos;
				updateContact = true;
			}
			//TODO: reset with some delay?
			ResetTriggered();
			ResetShot();
			return true;
		}
		else
		{
			return false;
		}
	}

	//DEBUG
	private bool editorDebugdraw = false;
	private bool updateContact = false;
	private Vector3 contactpos = Vector3.zero;
	void OnDrawGizmos()
	{
		if (updateContact)
		{
			updateContact = false;
			StopCoroutine("drawDelay");
			StartCoroutine("drawDelay");
		}
		
		if (editorDebugdraw)
		{
			Gizmos.DrawSphere(contactpos, 2.0f);
		}
	}
	
	IEnumerator drawDelay()
	{
		editorDebugdraw = true;
		yield return new WaitForSeconds(1.0f);
		editorDebugdraw = false;
	}
	
}
