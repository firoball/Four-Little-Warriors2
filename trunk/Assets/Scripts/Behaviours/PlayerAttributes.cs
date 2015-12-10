using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerAttributes : CollectableItemContainer
{
	public PlayerAttribute[] attributes;

	[SyncVar(hook="DeserializeDataStream")]
	public/*private*/ string m_dataStream = "";

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
			attributes[i].value = Math.Max(
				attributes[i].limitMin, attributes[i].value);
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
				//inform clients
				//RpcUpdateAttribute(index, attributes[index].value, attributes[index].max);
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
				//inform clients
				//RpcUpdateAttribute(index, attributes[index].value, attributes[index].max);
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

	/*[ClientRpc]
	private void RpcUpdateAttribute(int index, byte value, byte max)
	{
		//avoid over/underflow error in case someone fiddled around on the client
		if (index < attributes.Length && index >= 0)
		{
			attributes[index].value = value;
			attributes[index].max = max;
		}
	}*/

	public byte GetValue(Attributes type)
	{
		for(int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].type == type)
			{
				return attributes[i].value;
			}
		}
		return 0;
	}

	public byte SubtractValue(Attributes type, byte value)
	{
		for(int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].type == type)
			{
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
		}
		return 0;
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
		//Debug.Log("send serialized attributes "+m_dataStream);
	}

	private void DeserializeDataStream(string dataStream)
	{
		Debug.Log("Deserialize: "+name);
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
		//Debug.Log("received serialized attributes "+dataStream);
	}

	private void TriggerEvent(PlayerAttribute attribute)
	{
		if (m_eventSubscription != null)
		{
			//Debug.Log("Event OnAttributeChange " + attribute.type);
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

	//dynamic
	public byte value;
//	public byte min;
	public byte max;
}
