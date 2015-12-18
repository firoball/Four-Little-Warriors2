using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public abstract class ObjectEventManager : NetworkBehaviour, IObjectEventTarget 
{

	public bool enableTrigger = false;
	public bool enableCollided = false;
	public bool enableShot = false;
	public bool enableUsed = false;
	public bool ignoreParent = false;
	public bool destroyOnPositiveResult = false;
	public bool allowClientEvents = false;
	public bool disableRpcCalls = false;
	public float destroyDelay = 0.0f;

	private bool m_isTriggered = false;
	private bool m_isCollided = false;
	private bool m_isShot = false;
	private bool m_isUsed = false;

	void OnTriggerEnter(Collider collider)
	{
		if(!enableTrigger || m_isTriggered || !(isServer || allowClientEvents) || collider.gameObject == gameObject)
		{
			return;
		}
		if (collider.tag == "Destroyed")
		{
			return;
		}
		if (ignoreParent && gameObject.transform.parent != null && gameObject.transform.parent == collider.gameObject.transform)
		{
			return;
		}

		//Debug.Log("Trigger");
		m_isTriggered = true;
		bool result = OnEventTrigger(collider);
		if (result)
		{
			if (isServer && !disableRpcCalls)
			{
				RpcOnRemoteEventTrigger();
			}
			if (destroyOnPositiveResult)
			{
				Destroy();
			}
		}
		else
		{
			m_isTriggered = false;
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if(!enableCollided || m_isCollided || !(isServer || allowClientEvents) || collision.gameObject == gameObject)
		{
			return;
		}
		if (collision.gameObject.tag == "Destroyed")
		{
			return;
		}
		if (ignoreParent && gameObject.transform.parent != null && gameObject.transform.parent == collision.gameObject.transform)
		{
			return;
		}

		//Debug.Log("Collision");
		m_isCollided = true;
		bool result = OnEventCollision(collision);
		if (result)
		{
			if (isServer && !disableRpcCalls)
			{
				RpcOnRemoteEventCollision();
			}
			if (destroyOnPositiveResult)
			{
				Destroy();
			}
		}
		else
		{
			m_isCollided = false;
		}
	}

	public void OnRaycastShot(Collider collider)
	{
		if(!enableShot || m_isShot || !(isServer || allowClientEvents) || collider.gameObject == gameObject)
		{
			return;
		}
		if (ignoreParent && gameObject.transform.parent != null && gameObject.transform.parent == collider.gameObject.transform)
		{
			return;
		}

		//Debug.Log("Shot");
		m_isShot = true;
		bool result = OnEventShot(collider);
		if (result)
		{
			if (isServer && !disableRpcCalls)
			{
				RpcOnRemoteEventShot();
			}
			if (destroyOnPositiveResult)
			{
				Destroy();
			}
		}
		else
		{
			m_isShot = false;
		}
	}

	public void OnRaycastUse(Collider collider)
	{
		if(!enableUsed || m_isUsed || !(isServer || allowClientEvents) || collider.gameObject == gameObject)
		{
			return;
		}
		if (ignoreParent && gameObject.transform.parent != null && gameObject.transform.parent == collider.gameObject.transform)
		{
			return;
		}

		//Debug.Log("Used");
		m_isUsed = true;
		bool result = OnEventUsed(collider);
		if (result)
		{
			if (isServer && !disableRpcCalls)
			{
				RpcOnRemoteEventUsed();
			}
			if (destroyOnPositiveResult)
			{
				Destroy();
			}
		}
		else
		{
			m_isUsed = false;
		}
	}
	
	protected void Destroy(float delay = 0.0f)
	{
		//Debug.Log("Destroy");
		if (delay > 0.0f)
		{
			destroyDelay = delay;
		}

		if (destroyDelay > 0.0f)
		{
			StartCoroutine(WaitDestroy());
		}
		else
		{
			DestroyNow();
		}
	}

	protected void Lock()
	{
		enableTrigger = false;
		enableCollided = false;
		enableShot = false;
		enableUsed = false;
	}

	protected void Reset()
	{
		m_isTriggered = false;
		m_isShot = false;
		m_isUsed = false;
		m_isCollided = false;
	}

	protected void ResetTriggered()
	{
		m_isTriggered = false;
	}

	protected void ResetCollided()
	{
		m_isCollided = false;
	}

	protected void ResetShot()
	{
		m_isShot = false;
	}

	protected void ResetUsed()
	{
		m_isUsed = false;
	}

	protected virtual bool OnEventTrigger(Collider collider)
	{
		Debug.LogWarning(this.name + ": OnEventTrigger not implemented.");
		return false;
	}

	protected virtual bool OnEventCollision(Collision collision)
	{
		Debug.LogWarning(this.name + ": OnEventCollision not implemented.");
		return false;
	}
	
	protected virtual bool OnEventShot(Collider collider)
	{
	
		Debug.LogWarning(this.name + ": OnEventShot not implemented.");
		return false;
	}
	
	protected virtual bool OnEventUsed(Collider collider)
	{
		Debug.LogWarning(this.name + ": OnEventUsed not implemented.");
		return false;
	}
	
	[ClientRpc]
	protected virtual void RpcOnRemoteEventTrigger()
	{
	}

	[ClientRpc]
	protected virtual void RpcOnRemoteEventCollision()
	{
	}
	
	[ClientRpc]
	protected virtual void RpcOnRemoteEventShot()
	{
	}
	
	[ClientRpc]
	protected virtual void RpcOnRemoteEventUsed()
	{
	}

	private void DestroyNow()
	{
		//Debug.Log("DestroyNow");
		if (isServer)
		{
			NetworkServer.Destroy(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	private IEnumerator WaitDestroy()
	{
		gameObject.tag = "Destroyed";
		yield return new WaitForSeconds(destroyDelay);
		DestroyNow();
	}

}
