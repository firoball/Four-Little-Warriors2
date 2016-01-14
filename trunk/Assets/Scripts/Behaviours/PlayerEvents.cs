using UnityEngine;
using UnityEngine.EventSystems;
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
		//on client side no extra inhibition is needed - always 0.0f
		PushEvent(direction, position, duration, 0.0f);
	}
	
	private void PushEvent(Vector3 direction, Vector3 position, float duration, float inhibition)
	{
//		Debug.Log("PlayerEvents: PushEvent "+direction);
		ExecuteEvents.Execute<IPushEventTarget>(gameObject, null,(x,y)=>x.OnPush(
			direction, position, duration, inhibition)
		                                        );

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
				float duration = 0.5f; //something for now
				//trigger push event on clients
				if (isServer && !isClient)
				{
					RpcOnPushEvent(direction, transform.position, duration); //only send RPC if not in host mode
				}
				/* add latency of client since pushing on client will happen with delay
				 * which will force PlayerMovement to ignore input states for a longer while
				 * so player does not move while still being pushed on the client.
				 * Without this feature, the player would "jump" around on client side and perform movement
				 * invisible to client
				 */

				float inhibition = PlayerManager.GetLatency(gameObject);
				PushEvent(direction, transform.position, duration, inhibition); //update server

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
			Gizmos.color = Color.red;
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
