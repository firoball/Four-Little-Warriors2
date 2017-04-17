using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System;

public class PlayerInput : MonoBehaviour 
{
	internal static readonly BitVector32.Section sectMotionH = BitVector32.CreateSection(0x2F);
	internal static readonly BitVector32.Section sectMotionV = BitVector32.CreateSection(0x2F, sectMotionH);
	internal static readonly BitVector32.Section sectRun = BitVector32.CreateSection(0x1F, sectMotionV);
	internal static readonly BitVector32.Section sectJump = BitVector32.CreateSection(0x1F, sectRun);
	internal static readonly BitVector32.Section sectUse = BitVector32.CreateSection(0x1F, sectJump);
	internal static readonly BitVector32.Section sectAttack = BitVector32.CreateSection(0x1F, sectUse);

	private BitVector32 inputs;

	/*private float motionH;
	private float motionV;
	private float run;
	private float jump;
	private float use;
	private float attack;*/
	private InputData inputData;

	// Use this for initialization
	void Start () 
	{
		inputs = new BitVector32();
		inputData.motionH = 0.0f;
		inputData.motionV = 0.0f;
		inputData.run = 0.0f;
		inputData.jump = 0.0f;
		inputData.use = 0.0f;
		inputData.attack = 0.0f;
	}
	public bool shoot; //debug	
	private int i = 0;
	private int j = 0;
	public void Process()
	{
		float inMotionH = Input.GetAxis("Horizontal");
		float inMotionV = Input.GetAxis("Vertical");
		float inRun = Convert.ToSingle(Input.GetButton("Run"));
		float inJump = Convert.ToSingle(Input.GetButton("Jump"));
		float inUse = Convert.ToSingle(Input.GetButton("Use"));
		float inAttack = Convert.ToSingle(Input.GetButton("Attack"));
		shoot=Input.GetButton("Attack");
		///DEBUG
//		PlayerStats stats = GetComponent<PlayerStats>();		
//		if (stats.id == 2)
		NetClPlayerManager plm = GetComponent<NetClPlayerManager>();
		if(plm.playerControllerId == 1)
		{
			i++;
			if (i > 20)
			{
				i = 0;
				j = 2-j;
			}
			inMotionV = Convert.ToSingle(j-1);
		}
		///DEBUG

		//Debug.Log (inMotionH);
		if (inMotionH >= 0.1f)
			inputs[sectMotionH] = 2;
		else if (inMotionH <= -0.1f)
			inputs[sectMotionH] = 0;
		else
			inputs[sectMotionH] = 1;

		if (inMotionV >= 0.1f)
			inputs[sectMotionV] = 2;
		else if (inMotionV <= -0.1f)
			inputs[sectMotionV] = 0;
		else
			inputs[sectMotionV] = 1;

		if (inRun > 0.0f)
			inputs[sectRun] = 1;
		else
			inputs[sectRun] = 0;

		if (inJump > 0.0f)
			inputs[sectJump] = 1;
		else
			inputs[sectJump] = 0;
		
		if (inUse > 0.0f)
			inputs[sectUse] = 1;
		else
			inputs[sectUse] = 0;
		
		if (inAttack > 0.0f)
			inputs[sectAttack] = 1;
		else
			inputs[sectAttack] = 0;
		
		Decode();
		//hack
		//motionH = inMotionH;
		//motionV = inMotionV;
		//run = inRun;
		//jump = inJump;
	}

	public void Process(int data)
	{
		inputs = new BitVector32(data);
		Decode();
	}

	public InputData GetInputData()
	{
		return inputData;
	}

	public int Data()
	{
		return inputs.Data;
	}

	private void Decode()
	{
		inputData.motionH = Convert.ToSingle(inputs[sectMotionH] - 1);
		inputData.motionV = Convert.ToSingle(inputs[sectMotionV] - 1);
		inputData.run = Convert.ToSingle(inputs[sectRun]);
		inputData.jump = Convert.ToSingle(inputs[sectJump]);
		inputData.use = Convert.ToSingle(inputs[sectUse]);
		inputData.attack = Convert.ToSingle(inputs[sectAttack]);
	}

	/*public float MotionH
	{
		get {return inputData.motionH;}
	}

	public float MotionV
	{
		get {return inputData.motionV;}
	}
	
	public float Run
	{
		get {return inputData.run;}
	}
	
	public float Jump
	{
		get {return inputData.jump;}
	}
	
	public float Use
	{
		get {return inputData.use;}
	}
	
	public float Attack
	{
		get {return inputData.attack;}
	}*/
	
}
