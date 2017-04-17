using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class DamageDealer : MonoBehaviour 
{
	[SerializeField]
	private AttackTypes m_attackType = AttackTypes.NONE;
	[SerializeField]
	private Attributes m_attributeType = Attributes.LIFE;
	[SerializeField][Range(0,255)]
	private byte m_minDamage = 0;
	[SerializeField][Range(0,255)]
	private byte m_maxDamage = 0;
	[SerializeField]
	private float m_knockback = 0.0f;
	[SerializeField]
	private bool m_enableOnTouch = false;

	public AttackTypes AttackType 
	{
		get {return m_attackType;}
		set {m_attackType = value;} 
	}

	public Attributes AttributeType 
	{
		get {return m_attributeType;}
	}

	public byte Damage 
	{
		get {
			if (m_minDamage < m_maxDamage)
			{
				float damage = UnityEngine.Random.value * Convert.ToSingle(m_maxDamage - m_minDamage);
				return Convert.ToByte(Convert.ToByte(damage) + m_minDamage);
			}
			else
			{
				return m_minDamage;
			}
		}

		set {
			m_minDamage = Convert.ToByte(Math.Min(m_minDamage + value, Byte.MaxValue));
			m_maxDamage = Convert.ToByte(Math.Min(m_minDamage + value, Byte.MaxValue));
		}
	
	}

	public float Knockback 
	{
		get {return m_knockback;}
		set {m_knockback = value;}
	}

	public bool EnableOnTouch 
	{
		get {return m_enableOnTouch;}
	}
}
