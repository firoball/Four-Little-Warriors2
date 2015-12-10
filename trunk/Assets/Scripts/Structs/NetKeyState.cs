using UnityEngine;
using System.Collections;

public struct NetKeyState
{
	public int messageId;
	/*public float horizontal;
	public float vertical;
	public float run;
	public float jump;*/
	//public int inputData;
	public int inputData;

	public NetKeyState(int id, int input) 
	{
		messageId = id;
		//clumsy
		/*
		horizontal = input.MotionH;
		vertical = input.MotionV;
		run = input.Run;
		jump = input.Jump;
		inputData = input.Data();*/
		inputData = input;
	}
	
}
