using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerMovement))]
[RequireComponent (typeof(PlayerInput))]
public class LocalPlayerManager : MonoBehaviour 
{

	private PlayerMovement movement;
	private PlayerInput input;

	void Start () 
	{
		movement = gameObject.GetComponent<PlayerMovement>();
		input = gameObject.GetComponent<PlayerInput>();

		gameObject.name = "Player (Local)";
		//PlayerStats stats = GetComponent<PlayerStats>();
		//if (stats && stats.initialized)
		//{
			GameObject hudConfig = GameObject.Find("HudConfiguration");
			if (hudConfig != null)
			{
				hudConfig.SendMessage("SetupHud", gameObject);
			}
			else
			{
				Debug.Log("Object 'HudConfiguration' not found. No Hud available.");
			}
		//	gameObject.name += " id: " + stats.id;
		//}
	}
	
	void Update () 
	{
		input.Process();			
		movement.ProcessInputs(input.GetInputData(), Time.deltaTime);
		movement.ProcessActions(input.GetInputData());
		movement.ProcessEvents();
	}
}
