using UnityEngine;
using System.Collections;

public class TrapFireBeam : ObjectEventManager 
{
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
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
