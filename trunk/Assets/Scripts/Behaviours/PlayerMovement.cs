using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEditor;

[RequireComponent (typeof(CharacterController))]
[RequireComponent (typeof(PlayerAttributes))]
public class PlayerMovement : MonoBehaviour, IPushEventTarget 
{

	public float walkSpeed = 10.0f;
	public float slideSpeed = 10.0f;
	public float runMultiplier = 2.0f;
	public float sensitivityX = 50.0f;
	public float fallSpeed = 20.0f;
	public float jumpSpeed = 40.0f;
	public float jumpTime = 0.3f;
	public GameObject shot;
	
	public/*private*/ PlayerProperties m_properties;
	private CharacterController m_controller;
	private Transform m_launcher;
	private PlayerAttributes m_attributes;
	private float m_pushTimer;
	private float m_pushInhibition;
	private Vector3 m_pushDirection;
	private Vector3 m_pushPosition;
	private bool m_pushRequested;
	private bool m_lastPushRequested;

	private const float c_pushDelay = 0.2f;
	private const float c_runStaminaCost = -5.0f;
	private const float c_idleStaminaRecovery = 2.0f;
	private const float c_walkStaminaRecovery = 1.0f;

	void Awake()
	{
		m_pushTimer = 0.0f;
		m_pushInhibition = 0.0f;
		m_pushDirection = Vector3.zero;
		m_pushRequested = false;
		m_lastPushRequested = false;
		m_controller = gameObject.GetComponent<CharacterController>();
		m_attributes = gameObject.GetComponent<PlayerAttributes>();
		m_launcher = transform.Find("ProjectileLauncher");
		if (m_launcher == null)
		{
			m_launcher = transform; // use player transform as launcher
		}
	}

	void Start () 
	{
		Debug.Log("PlayerMovement Start");

		m_properties.position = transform.position;
		m_properties.rotation = transform.rotation;
		m_properties.jumpReady = false;
		m_properties.isPushed = false;
		m_properties.isRunning = false;
		m_properties.isGrounded = false;
		m_properties.jumpTimer = 0.0f;
		m_properties.stamina = Convert.ToSingle(m_attributes.GetValue(Attributes.STAMINA));
	}

	private float ProcessJump(float jump, bool isSliding, float timeStep)
	{
		float ret;

		//allow jumping only after jump button release, avoid perma-hopping
		if(jump <= 0.0f && !isSliding)
		{
			m_properties.jumpReady = true;
		}
				
		//jump is active, count to jumpTime and wait until ground is hit
		if (m_properties.jumpTimer > 0.0f)
		{
			m_properties.jumpTimer += timeStep;
			if (m_properties.jumpTimer > jumpTime)
			{
				m_properties.jumpTimer = 0.0f;
			}
			ret = jumpSpeed;
		}
		else
		{
			ret = 0.0f;
		}

		//start jump - controller must be grounded, jumping must be allowed
		if(jump >= 1.0f && m_properties.jumpReady && m_controller.isGrounded)
		{
			m_properties.jumpReady = false;
			m_properties.jumpTimer += timeStep;
		}
		
		return ret;
	}

	private bool ProcessSliding(out Vector3 slideDirection, float run)
	{
		RaycastHit hit;
		Vector3 colliderSpherePos = transform.position + m_controller.center;
		colliderSpherePos.y += m_controller.radius - m_controller.height * 0.5f;
		if (Physics.SphereCast(colliderSpherePos, m_controller.radius, Vector3.down, out hit, 200.0f))
		{
			if (m_controller.isGrounded || m_properties.jumpTimer > 0.0f)
			{
				if (hit.normal.y < 0.5f)
				{
					Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up);
					Vector3 down = Vector3.Cross(hit.normal, tangent);
					slideDirection = down * slideSpeed * run;
					return true;
				}
			}
		}
		slideDirection = Vector3.zero;
		return false;
	}

	private void ProcessPush(float timeStep)
	{
		/* Reconciliation is deactivated while pushing is active, so variables do not need to be stored in keyframes */
		if (m_pushRequested)
		{
			if (m_lastPushRequested != m_pushRequested)
			{
				transform.position = m_pushPosition;
			}
//			Debug.Log("m_pushTimer "+m_pushTimer + " " + m_pushInhibition);
			m_pushTimer -= timeStep;
			if (m_pushTimer <= 0.0f)
			{
				m_pushDirection = Vector3.zero;
			}
			//add some idle time
			if (m_pushTimer <= -m_pushInhibition)
			{
				m_pushTimer = 0.0f;
				m_pushInhibition = 0.0f;
				m_pushRequested = false;
			}
		}
		m_properties.isPushed = m_pushRequested;
		m_lastPushRequested = m_pushRequested;
	}

	private float ProcessRun(InputData inputData, float timeStep)
	{
		float runSpeed;
		float staminaChange;

		//manually sync stamina server side - attribute might have changed externally
		m_properties.stamina = m_attributes.SyncGetValue(Attributes.STAMINA, m_properties.stamina);

		if (
			inputData.run > 0.0f && m_properties.stamina >= 1.0f &&
			(inputData.motionH != 0.0f || inputData.motionV != 0.0f)
		    )
		{
			staminaChange = timeStep * c_runStaminaCost;
			runSpeed = inputData.run * runMultiplier;
			m_properties.isRunning = true;
		}
		else
		{
			runSpeed = 1.0f;
			m_properties.isRunning = false;

			if (inputData.motionH != 0.0f || inputData.motionV != 0.0f)
			{
				if (inputData.run < 1.0f)
				{
					staminaChange = timeStep * c_walkStaminaRecovery;
				}
				else
				{
					staminaChange = 0.0f;
				}
			}
			else
			{
				staminaChange = timeStep * c_idleStaminaRecovery;
			}
		}
		m_properties.stamina += staminaChange;
		m_properties.stamina = m_attributes.SyncSetValue(Attributes.STAMINA, m_properties.stamina);

		return runSpeed;
	}

	public void ProcessInputs(InputData inputData, float timeStep)
	{
		ProcessPush(timeStep);
		float run = ProcessRun(inputData, timeStep);

		//float run = Mathf.Max(1.0f, inputData.run * runMultiplier);
		Vector3 slideDirection;
		bool sliding = ProcessSliding(out slideDirection, run);

		Vector3 moveDirection;
		if (!m_properties.isPushed)
		{
			float jump = ProcessJump(inputData.jump, sliding, timeStep);
			byte speedBonus = m_attributes.GetValue(Attributes.SPEED);
			float speedMultiplier = 1.0f + 0.1f * Convert.ToSingle(speedBonus);

			transform.Rotate(0, inputData.motionH * sensitivityX * timeStep, 0);

			moveDirection = transform.rotation * new Vector3(
				0.0f, 
				jump - fallSpeed, 
				inputData.motionV * walkSpeed * run * speedMultiplier
				);
		}
		else
		{
			moveDirection = new Vector3(0.0f, -fallSpeed, 0.0f);
		}
		m_controller.Move((moveDirection + slideDirection + m_pushDirection) * timeStep);
		m_properties.isGrounded = m_controller.isGrounded;
		//if (m_properties.isPushed) Debug.Log(transform.position);
	}

	private bool m_lastAttack = false;
	private bool m_lastUse = false;

	private bool editorDebugdraw = false;
	private Vector3 fwd;
	private Vector3 pos;
	void OnDrawGizmos()
	{
		if (m_lastAttack)
		{
			fwd = transform.TransformDirection(Vector3.forward);
			pos = GetComponent<Collider>().bounds.center;
			StopCoroutine("drawDelay");
			StartCoroutine("drawDelay");
		}

		if (editorDebugdraw)
		{
			Gizmos.DrawLine(pos, pos+fwd*4.0f);
			Gizmos.DrawSphere(pos+fwd*4.0f, 2.0f);
		}
	}

	IEnumerator drawDelay()
	{
		editorDebugdraw = true;
		yield return new WaitForSeconds(1.0f);
		editorDebugdraw = false;
	}

	private GameObject target;
	private bool activeAttackEvent = false;
	private bool activeUseEvent = false;

	public void ProcessActions(InputData inputData)
	{
		RaycastHit hit;
		if (!m_lastAttack)
		{
			if (inputData.attack > 0.0f)
			{
				m_lastAttack = true;
				Vector3 pos = GetComponent<Collider>().bounds.center;
				Vector3 fwd = transform.TransformDirection(Vector3.forward);
				if (Physics.SphereCast(pos, 2.0f, fwd, out hit, 4.0f)) //TODO: use real attack range
				{
					target = hit.transform.gameObject;
					activeAttackEvent = true;
				}
				else
				{
					//TODO: temporary implementation of projectile launcher
					GameObject objProjectile = (GameObject)Instantiate(shot, m_launcher.position, m_launcher.rotation);
					Projectile projectile = objProjectile.GetComponent<Projectile>();
					if (projectile != null)
					{
						byte damageLevel = m_attributes.GetValue(Attributes.DAMAGE);
						byte damage = Convert.ToByte(4 + damageLevel * 2);
						float range = 7.0f + Convert.ToSingle(m_attributes.GetValue(Attributes.RANGE)) * 1.5f;
						projectile.Setup(damage, range, gameObject, Convert.ToInt32(damageLevel));
					}
					else
					{
						Debug.LogWarning("PlayerMovement: Spawned projectile misses Projectile component.");
					}
					//inform all clients about spawn if running on server, otherwise just spawn (local player only)
					if (UnityEngine.Networking.NetworkServer.active)
					{
						UnityEngine.Networking.NetworkServer.Spawn(objProjectile);
					}
				}
			}
		}
		else
		{
			if (inputData.attack <= 0.0f)
			{
				m_lastAttack = false;
			}
		}
	}

	public void ProcessEvents()
	{
		if (activeAttackEvent || activeUseEvent)
		{
			Debug.Log(this.name+" hit: "+target.name);
			//TODO: pick proper collider (filter isTrigger colliders)
			if (activeAttackEvent)
			{
				ExecuteEvents.Execute<IObjectEventTarget>(target, null,(x,y)=>x.OnRaycastShot(this.GetComponent<Collider>()));
				activeAttackEvent = false;
			}
			if (activeUseEvent)
			{
				ExecuteEvents.Execute<IObjectEventTarget>(target, null,(x,y)=>x.OnRaycastUse(this.GetComponent<Collider>()));
				activeUseEvent = false;
			}
		}
	}

	/* used for client interpolation */
	public void ProcessInterpolation(PlayerProperties lastProps, PlayerProperties currentProps, float time, bool extrapolate = false)
	{
		Vector3 posOld = lastProps.position;
		Quaternion rotOld = lastProps.rotation;
		float jumpOld = lastProps.jumpTimer;
		Vector3 posCur = currentProps.position;
		Quaternion rotCur = currentProps.rotation;
		float jumpCur = currentProps.jumpTimer;

		m_properties = currentProps; //make sure all non interpolated properties are up to date
		m_attributes.SyncSetValue(Attributes.STAMINA, m_properties.stamina);
		if (!extrapolate)
		{
			time = Mathf.Min(time, 1.0f);
		}

		if (time > 1.0f)
		{
			//extrapolate
			ProcessDirectMovement(posOld + (posCur - posOld) * time, rotCur, jumpCur);
		}
		else
		{
			//Interpolate
			Vector3 posLerped = Vector3.Lerp(posOld, posCur, time);
			Quaternion rotLerped = Quaternion.Slerp(rotOld, rotCur, time);
			float jumpLerped = Mathf.Lerp(jumpOld, jumpCur, time);
			ProcessDirectMovement(posLerped, rotLerped, jumpLerped);
		}
	}
	
	/*public Vector3 RAYCASTPOS;
	public Vector3 HITPOS;
	public float HITDIST;
	public float YDELTA;*/
	/* used for client interpolation */
	public void ProcessDirectMovement(Vector3 position, Quaternion rotation, float jumpTimer)
	{
		float yOldPos = transform.position.y;
		float yDelta = 0.0f;

		transform.position = position;
		transform.rotation = rotation;

		//setup sphere raycast origin. If player was moving downwards, use old y position
		//this will avoid faulty raycast as player might have been extrapolated below floor level
		Vector3 colliderSpherePos = transform.position + m_controller.center;
		colliderSpherePos.y += m_controller.radius - m_controller.height * 0.5f;
		if (yOldPos > position.y)
		{
			yDelta = yOldPos - position.y;
			colliderSpherePos.y += yDelta;
			//YDELTA = yDelta;
		}

		m_properties.isGrounded = false;
		RaycastHit hit;
		//RAYCASTPOS = colliderSpherePos;
		if (Physics.SphereCast(colliderSpherePos, m_controller.radius, Vector3.down, out hit, 200.0f/*controller.stepOffset*/))
		{
			//if (hit.collider != null && hit.collider.name != "Plane")
			//	Debug.Log("spherecast " + hit.collider.name + " " + hit.collider.isTrigger);
			/*HITPOS = hit.point;
			HITDIST = hit.distance;*/
			//Debug.Log ("distance: "+hit.distance);
			//limit y position if player is about to sink into floor
			float verticalOffset = 0.08f;//same as CharacterController skin width
/*			if (
				(transform.position.y - hit.point.y) >= 0.0f && //ignore any y pos rising, not corrected 
				(transform.position.y - hit.point.y) < (verticalOffset - 0.01f)
				)
			{
				Vector3 correctedPosition = transform.position;
				correctedPosition.y = hit.point.y + verticalOffset;
				Debug.Log("fallthrough prevented "+transform.position.y+" "+hit.point.y+" "+(transform.position.y-hit.point.y)+" "+verticalOffset);
				transform.position = correctedPosition;
			}*/
			Vector3 correctedPosition = transform.position;
			correctedPosition.y = Mathf.Max(hit.point.y + verticalOffset, transform.position.y);
			//Debug.Log("fallthrough prevented "+transform.position.y+" "+hit.point.y+" "+(transform.position.y-hit.point.y)+" "+verticalOffset);
			transform.position = correctedPosition;

			//player on or near floor (ignore skin width, take stepOffset into account)? set grounded
			if (transform.position.y - verticalOffset <= hit.point.y + m_controller.stepOffset)
			{
				m_properties.isGrounded = true;
			}
		}

		m_properties.jumpTimer = jumpTimer;
	}

	public void SetProperties(PlayerProperties properties)
	{
		m_properties = properties;
		transform.position = m_properties.position;
		transform.rotation = m_properties.rotation;
		//controller.isGrounded will be set indirectly due to transform update
	}

	public PlayerProperties GetProperties()
	{
		m_properties.position = transform.position;
		m_properties.rotation = transform.rotation;
		//props.isGrounded = controller.isGrounded;

		return m_properties;
	}

	public void OnPush(Vector3 direction, Vector3 position, float duration, float inhibition)
	{
		if (!m_pushRequested)
		{
			m_pushRequested = true;
			m_pushDirection = direction;
			m_pushPosition = position;
			m_pushTimer = Mathf.Max(0.0f, duration);
			m_pushInhibition = c_pushDelay + Mathf.Max(0.0f, inhibition);
		}
	}
	
}
