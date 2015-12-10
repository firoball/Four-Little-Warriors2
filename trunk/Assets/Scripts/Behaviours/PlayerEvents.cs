using UnityEngine;
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
	
	protected override void RpcOnRemoteEventTrigger()
	{
		Debug.Log("PLAYER Remote Trigger");
	}
	
	protected override void RpcOnRemoteEventShot()
	{
		Debug.Log("PLAYER Remote Shot");
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
			ResetTriggered();
			return true;
		}
		else
		{
			return false;
		}
	}
}
