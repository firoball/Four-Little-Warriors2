using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerAttributes : CollectableItemContainer
{
	public PlayerAttribute[] attributes;

	[SyncVar(hook="DeserializeDataStream")]
	private string m_dataStream = "";

	private EventSubscription m_eventSubscription = null;

	void Awake()
	{
		m_eventSubscription = GetComponent<EventSubscription>();
		for(int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].limitMax == 0)
			{
				attributes[i].limitMax = Byte.MaxValue;
			}
			attributes[i].value = Math.Max(attributes[i].limitMin, attributes[i].value);
			attributes[i].interimValue = 0.0f;
		}
	}

	void Start () 
	{
		//only for real clients, not local multiplayer (host mode)
		//make sure data from peer clients is also updated on start
		if (!isServer && isClient)
		{
			DeserializeDataStream(m_dataStream);
		}
	}

	/*public override void OnStartClient()
	{
		DeserializeDataStream(m_dataStream);
	}*/
	
	public void SubscribeEvents(GameObject subscriber)
	{		
		if (m_eventSubscription != null)
		{
			m_eventSubscription.SubscribeEvents(subscriber);
			//trigger initial update events
			for (int index = 0; index < attributes.Length; index++)
			{
				TriggerEvent(attributes[index]);
			}			
		}
	}

	public void UnSubscribeEvents(GameObject subscriber)
	{		
		if (m_eventSubscription != null)
		{
			m_eventSubscription.UnSubscribeEvents(subscriber);
		}
	}

	protected override bool OnCollect(CollectableProperties properties)
	{
		int index = Find (properties.type);
		if (index != -1)
		{
			if(properties.limitModifier)
			{
				if (attributes[index].max == attributes[index].limitMax)
				{
					//don't collect
					return false;
				}

				if ((attributes[index].limitMax - attributes[index].max) > properties.value)
				{
					attributes[index].max += properties.value;
				}
				else
				{
					attributes[index].max = attributes[index].limitMax;
				}
			}
			else
			{
				if (attributes[index].value == attributes[index].max)
				{
					//don't collect
					return false;
				}
				
				if ((attributes[index].max - attributes[index].value) > properties.value)
				{
					attributes[index].value += properties.value;
				}
				else
				{
					attributes[index].value = attributes[index].max;
				}
			}
			//trigger event
			TriggerEvent(attributes[index]);

			//inform clients
			SerializeDataStream();

			//collect
			return true;
		}

		//unknown item - don't collect
		return false;
	}

	protected override bool OnDrop(CollectableProperties properties)
	{
		//TODO: implementation :)
		return true;
	}	

	public byte GetValue(Attributes type)
	{
		int i = Find (type);
		if (i < 0)
		{
			return 0;
		}
		
		return attributes[i].value;
	}

	public byte SubtractValue(Attributes type, float value)
	{
		if (!isServer)
		{
			return 0;
		}
		
		int i = Find (type);
		if (i < 0)
		{
			return 0;
		}
		
		if (attributes[i].value == 0)
		{
			return attributes[i].value;
		}
		attributes[i].interimValue -= Mathf.Min(value, 0.0f);
		float roundedValue = Mathf.Ceil(attributes[i].interimValue);
		if (roundedValue > -1.0f)
		{
			return attributes[i].value;
		}
		attributes[i].interimValue = attributes[i].interimValue % roundedValue;
		byte resultingValue = Convert.ToByte(-roundedValue);
		return SubtractValue(i, resultingValue);
	}
	
	public byte SubtractValue(Attributes type, byte value)
	{
		if (!isServer)
		{
			return 0;
		}

		int i = Find (type);
		return SubtractValue(i, value);
	}
	
	private byte SubtractValue(int i, byte value)
	{
		if (i < 0)
		{
			return 0;
		}
		
		if (attributes[i].value > value)
		{
			attributes[i].value -= value;
		}
		else
		{
			attributes[i].value = 0;
		}
		//inform clients
		SerializeDataStream();
		//trigger event
		TriggerEvent(attributes[i]);
		return attributes[i].value;
	}

	public byte AddValue(Attributes type, float value)
	{
		if (!isServer)
		{
			return 0;
		}
		
		int i = Find (type);
		if (i < 0)
		{
			return 0;
		}
		
		if (attributes[i].value == attributes[i].max)
		{
			return attributes[i].value;
		}
		attributes[i].interimValue += Mathf.Max(value, 0.0f);
		float roundedValue = Mathf.Floor(attributes[i].interimValue);
		if (roundedValue < 1.0f)
		{
			return attributes[i].value;
		}
		attributes[i].interimValue = attributes[i].interimValue % roundedValue;
		byte resultingValue = Convert.ToByte(roundedValue);
		return AddValue(i, resultingValue);
	}

	public byte AddValue(Attributes type, byte value)
	{
		if (!isServer)
		{
			return 0;
		}
		
		int i = Find (type);
		return AddValue(i, value);		
	}

	private byte AddValue(int i, byte value)
	{
		if (i < 0)
		{
			return 0;
		}

		if (value == 0)
		{
			return attributes[i].value;
		}
		
		if (attributes[i].value <= Convert.ToByte(attributes[i].max - value))
		{
			attributes[i].value += value;
		}
		else
		{
			attributes[i].value = attributes[i].max;
		}
		//inform clients
		SerializeDataStream();
		//trigger event
		TriggerEvent(attributes[i]);
		return attributes[i].value;
	}
	/*public byte GetMax(Attributes type)
	{
		for(int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].type == type)
			{
				return attributes[i].max;
			}
		}
		return 0;
	}*/
	
	private int Find(Attributes type)
	{
		for(int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].type == type)
			{
				return i;
			}
		}

		return -1;
	}

	private void SerializeDataStream()
	{
		char[] value = new char[attributes.Length];
		char[] max = new char[attributes.Length];
		for (int i = 0; i < attributes.Length; i++)
		{
			value[i] = (char)attributes[i].value;
			max[i] = (char)attributes[i].max;
		}
		m_dataStream = new string(value) + new string(max);
	}

	private void DeserializeDataStream(string dataStream)
	{
		//each attribute requires 2 byte
		if (dataStream.Length != 2 * attributes.Length)
		{
			return;
		}
		m_dataStream = dataStream;
		char[] values = dataStream.ToCharArray();
		for (int index = 0; index < attributes.Length; index++)
		{
			if (attributes[index].value != (byte)values[index] || attributes[index].max != (byte)values[attributes.Length + index] )
			{
				attributes[index].value = (byte)values[index];
				attributes[index].max = (byte)values[attributes.Length + index];
				TriggerEvent(attributes[index]);
			}
		}
	}

	private void TriggerEvent(PlayerAttribute attribute)
	{
		if (m_eventSubscription != null)
		{
			m_eventSubscription.ExecuteEvent<IAttributeEventTarget>(null,(x,y)=>x.OnAttributeChange(attribute));
		}
	}
}

[Serializable]
public struct PlayerAttribute
{
	//static
	public Attributes type;
	public byte limitMin;
	public byte limitMax;
	public bool useClientPrediction;

	//dynamic - synched
	public byte value;
//	public byte min;
	public byte max;

	//dynamic - local
	public float interimValue;
	public byte predictedValue;
}
