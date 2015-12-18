using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[Serializable]
public struct PlayerProperties
{
	//active data
	public bool jumpReady;
	public float jumpTimer;
	public bool isGrounded;
	public bool isPushed;

	//transform copy
	public Vector3 position;
	public Quaternion rotation;

	public static bool operator !=(PlayerProperties props1, PlayerProperties props2)
	{
		Vector3 deltaPos = props1.position - props2.position;
		Vector3 deltaRot = props1.rotation.eulerAngles - props2.rotation.eulerAngles;

		if (
			(props1.jumpReady != props2.jumpReady) ||
			(props1.isPushed != props2.isPushed) ||
			(props1.isGrounded != props2.isGrounded) ||
			(Mathf.Abs(props1.jumpTimer - props2.jumpTimer) > 0.1f) ||
			(deltaPos.sqrMagnitude > (0.1f * 0.1f)) ||
			(deltaRot.sqrMagnitude > 1.0f)
		)
		{
			Debug.Log("pos "+props1.position+"/"+props2.position+" rot "
			          +props1.rotation.eulerAngles+"/"+props2.rotation.eulerAngles);
			/*Debug.Log("dpos "+deltaPos.sqrMagnitude+" drot "+deltaRot.sqrMagnitude
			          + " jt "+Mathf.Abs(props1.jumpTimer - props2.jumpTimer));*/
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool operator ==(PlayerProperties props1, PlayerProperties props2)
	{
		if (props1 != props2)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PlayerProperties))
		{
			return false;
		}
		
		PlayerProperties pP = (PlayerProperties) obj;		

		if (pP == this)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	public override int GetHashCode()
	{
		int hash = 13;
		hash = (hash * 7) + jumpReady.GetHashCode();
		hash = (hash * 7) + isPushed.GetHashCode();
		hash = (hash * 7) + isGrounded.GetHashCode();
		hash = (hash * 7) + jumpTimer.GetHashCode();
		hash = (hash * 7) + position.GetHashCode();
		hash = (hash * 7) + rotation.GetHashCode();

		return hash;	
	}

	public void NetworkSerialize(NetworkWriter writer)
	{
		writer.Write (position);
		writer.Write (rotation.eulerAngles);
		writer.Write (jumpReady);
		writer.Write (isPushed);
		writer.Write (isGrounded);
		writer.Write (jumpTimer);
	}

	public void NetworkDeserialize(NetworkReader reader)
	{
		position = reader.ReadVector3();
		rotation = Quaternion.Euler(reader.ReadVector3());
		jumpReady = reader.ReadBoolean();
		isPushed = reader.ReadBoolean();
		isGrounded = reader.ReadBoolean();
		jumpTimer = reader.ReadSingle();
	}
}

