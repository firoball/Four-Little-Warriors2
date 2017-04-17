using UnityEngine;
using System;

[Serializable]
public class MovementSettings
{
	//movement parameters
	public float walkSpeed = 10.0f;
	public float slideSpeed = 10.0f;
	public float runMultiplier = 2.0f;
	public float sensitivityX = 50.0f;
	public float fallSpeed = 20.0f;
	public float jumpSpeed = 40.0f;
	public float jumpTime = 0.3f;
	public float pushDelay = 0.2f;

	//stamina consumption and recovery parameters
	public float runStaminaCost = -5.0f;
	public float idleStaminaRecovery = 2.0f;
	public float walkStaminaRecovery = 1.0f;

	//action parameters
	public float[] attackDuration = new float[3] {0.5f, 0.5f, 0.5f};
	public float[] attackTrigger = new float[3] {0.3f, 0.3f, 0.3f};
	public float[] comboTrigger = new float[3] {0.4f, 0.4f, 0.4f};
	public float hitDuration = 0.3f;

}
