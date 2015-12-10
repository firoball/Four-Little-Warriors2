using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TriggerSwitch : ObjectEventManager 
{
	public TriggerSwitchType switchType = TriggerSwitchType.ONOFF;
	public float releaseTimer = 1.0f;

	private const float onoffDelay = 0.1f;
	private Animator animator;
	private bool activated = false;

	void Start () 
	{
		animator = GetComponent<Animator>();
	}
	
	protected override bool OnEventTrigger(Collider collider)
	{
		return Trigger ();
	}
	
	protected override bool OnEventShot(Collider collider)
	{
		return Trigger ();
	}
	
	protected override bool OnEventUsed(Collider collider)
	{
		return Trigger ();
	}
	
	private bool Trigger()
	{
		switch(switchType)
		{
		case TriggerSwitchType.ONOFF:
			StartCoroutine(WaitOnOff());
			break;

		case TriggerSwitchType.TIMED:
			StartCoroutine(WaitTimer());
			break;

		case TriggerSwitchType.SINGLE:
			break;

		default:
			return false;

		}

		return true;
	}
	
	protected override void RpcOnRemoteEventTrigger()
	{
		RemoteTrigger();
	}
	
	protected override void RpcOnRemoteEventShot()
	{
		RemoteTrigger();
	}
	
	protected override void RpcOnRemoteEventUsed()
	{
		RemoteTrigger();
	}

	[ClientRpc]
	private void RpcRemoteTrigger()
	{
		RemoteTrigger();
	}

	private void RemoteTrigger()
	{
		activated = !activated;
		Debug.Log ("switch triggered: "+activated);
		if (animator != null)
		{
			if (activated)
			{
				animator.SetTrigger("enableTrigger");
			}
			else
			{
				animator.SetTrigger("disableTrigger");
			}
		}
	}


	private IEnumerator WaitOnOff()
	{
		yield return new WaitForSeconds(onoffDelay);
		//allow new events to come
		Reset();
	}

	private IEnumerator WaitTimer()
	{
		yield return new WaitForSeconds(releaseTimer);
		RpcRemoteTrigger();
		activated = false;
		//allow new events to come
		Reset();
	}
}

public enum TriggerSwitchType : int
{
	NONE = 0,
	ONOFF = 1,
	TIMED = 2,
	SINGLE = 3
}