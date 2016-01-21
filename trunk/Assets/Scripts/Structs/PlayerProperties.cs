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
	public float stamina;
	public bool isGrounded;
	public bool isPushed;
	public bool isRunning;

	//transform copy
	public Vector3 position;
	public Quaternion rotation;

	public static bool operator !=(PlayerProperties props1, PlayerProperties props2)
	{
		Vector3 deltaPos = props1.position - props2.position;
		Vector3 deltaRot = props1.rotation.eulerAngles - props2.rotation.eulerAngles;

		bool sync = false;

		if (props1.jumpReady != props2.jumpReady)
		{
			sync = true;
			Debug.Log("Sync jumpReady: " + props1.jumpReady + "/" + props2.jumpReady);
		}
		if (props1.isPushed != props2.isPushed)
		{
			sync = true;
			Debug.Log("Sync isPushed: " + props1.isPushed + "/" + props2.isPushed);
		}
		if (props1.isRunning != props2.isRunning)
		{
			sync = true;
			Debug.Log("Sync isRunning: " + props1.isRunning + "/" + props2.isRunning);
		}
		if (props1.isGrounded != props2.isGrounded)
		{
			sync = true;
			Debug.Log("Sync isGrounded: " + props1.isGrounded + "/" + props2.isGrounded);
		}
		if (Mathf.Abs(props1.jumpTimer - props2.jumpTimer) > 0.1f)
		{
			sync = true;
			Debug.Log("Sync jumpTimer: " + props1.jumpTimer + "/" + props2.jumpTimer);
		}
		if (Mathf.Abs(props1.stamina - props2.stamina) > 0.1f)
		{
			sync = true;
			Debug.Log("Sync stamina: " + props1.stamina + "/" + props2.stamina);
		}
		if (deltaPos.sqrMagnitude > (0.1f * 0.1f))
		{
			sync = true;
			Debug.Log("Sync position: " + props1.position + "/" + props2.position);
		}
		if (deltaRot.sqrMagnitude > 1.0f)
		{
			sync = true;
			Debug.Log("Sync rotation: " + props1.rotation.eulerAngles + "/" + props2.rotation.eulerAngles);
		}

		return sync;
/*		if (
			(props1.jumpReady != props2.jumpReady) ||
			(props1.isPushed != props2.isPushed) ||
			(props1.isRunning != props2.isRunning) ||
			(props1.isGrounded != props2.isGrounded) ||
			(Mathf.Abs(props1.jumpTimer - props2.jumpTimer) > 0.1f) ||
			(Mathf.Abs(props1.stamina - props2.stamina) > 0.1f) ||
			(deltaPos.sqrMagnitude > (0.1f * 0.1f)) ||
			(deltaRot.sqrMagnitude > 1.0f)
		)
		{
			return true;
		}
		else
		{
			return false;
		}*/
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
		hash = (hash * 7) + isRunning.GetHashCode();
		hash = (hash * 7) + isGrounded.GetHashCode();
		hash = (hash * 7) + jumpTimer.GetHashCode();
		hash = (hash * 7) + stamina.GetHashCode();
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
		writer.Write (isRunning);
		writer.Write (isGrounded);
		writer.Write (jumpTimer);
		writer.Write (stamina);
	}

	public void NetworkDeserialize(NetworkReader reader)
	{
		position = reader.ReadVector3();
		rotation = Quaternion.Euler(reader.ReadVector3());
		jumpReady = reader.ReadBoolean();
		isPushed = reader.ReadBoolean();
		isRunning = reader.ReadBoolean();
		isGrounded = reader.ReadBoolean();
		jumpTimer = reader.ReadSingle();
		stamina = reader.ReadSingle();
	}
}

