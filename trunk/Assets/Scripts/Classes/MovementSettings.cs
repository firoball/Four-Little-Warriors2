using UnityEngine;
using System;

[Serializable]
public class MovementSettings
{
	public float walkSpeed = 10.0f;
	public float slideSpeed = 10.0f;
	public float runMultiplier = 2.0f;
	public float sensitivityX = 50.0f;
	public float fallSpeed = 20.0f;
	public float jumpSpeed = 40.0f;
	public float jumpTime = 0.3f;
	public float pushDelay = 0.2f;
	public float runStaminaCost = -5.0f;
	public float idleStaminaRecovery = 2.0f;
	public float walkStaminaRecovery = 1.0f;

}
