using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct CollectableProperties
{
	public string name;
	public Attributes type;
	public byte value;
	public bool singleUsage;
	public bool limitModifier;
}
