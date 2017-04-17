using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FixRotation : NetworkBehaviour 
{
	[SyncVar]
	public Vector3 rotation;

	public override void OnStartServer()		
	{
		rotation = transform.rotation.eulerAngles;
	}

	public override void OnStartClient()
	{
		transform.rotation = Quaternion.Euler(rotation);
	}
}
