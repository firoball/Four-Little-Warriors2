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

	public float SyncGetValue(Attributes type, float value)
	{
		//server only - use for manual synching
		if (!isServer)
		{
			return value;
		}

		int i = Find (type);
		if (i < 0)
		{
			return 0;
		}

		if (!attributes[i].disableSync)
		{
			return value;
		}

		float originalValue = Convert.ToSingle(attributes[i].value);
		float roundedValue = Mathf.Floor(value);
		float delta = originalValue - roundedValue;

		return value + delta;
	}

	public float SyncSetValue(Attributes type, float value)
	{
		//use for manual synching on client and server side
		int i = Find (type);
		if (i < 0)
		{
			return 0.0f;
		}
		
		//only allow direct assignment on client if no automatic synching is done
		if (isServer || attributes[i].disableSync)
		{
			value = Mathf.Min(Mathf.Max(0.0f, value), Convert.ToSingle(attributes[i].max));
			attributes[i].value = Convert.ToByte(Mathf.FloorToInt(value));
			if (isServer && !attributes[i].disableSync)
			{
				//inform clients
				SerializeDataStream();
			}
			//trigger event
			TriggerEvent(attributes[i]);
		}

		return value;
	}

	public byte SubtractValue(Attributes type, byte value)
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

	public byte AddValue(Attributes type, byte value)
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
			if (
				(!attributes[index].disableSync) &&
				(attributes[index].value != (byte)values[index] || attributes[index].max != (byte)values[attributes.Length + index] )
				)
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
	public bool disableSync;

	//dynamic - synched
	public byte value;
//	public byte min;
	public byte max;
}
