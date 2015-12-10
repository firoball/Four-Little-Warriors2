using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class HudAttributeDisplay : MonoBehaviour, IAttributeEventTarget
{
	private Image m_bar;
	private Text m_digits;
	private string m_textTemplate;
	private byte m_value = 0;

	public Attributes valueType = Attributes.NONE;
	public bool hideIfValueZero = false;

	void Awake()
	{
		foreach (Transform child in transform)
		{
			Image img = child.GetComponent<Image>();
			if (img != null && img.type == Image.Type.Filled)
			{
				m_bar = img;
			}

			Text txt = child.GetComponent<Text>();
			if (txt != null && txt.name == "Digits")
			{
				m_digits = txt;
				m_textTemplate = m_digits.text;
			}

			if (hideIfValueZero && m_value == 0)
			{
				child.gameObject.SetActive(false);
			}
		}
	}

	public void OnAttributeChange(PlayerAttribute attribute)
	{
		if (attribute.type == valueType)
		{
			if (m_digits != null)
			{
				string digits = m_textTemplate;
				digits = digits.Replace("[value]", attribute.value.ToString());
				digits = digits.Replace("[max]", attribute.max.ToString());
				m_digits.text = digits;
			}

			if (m_bar != null)
			{
				m_bar.fillAmount = Mathf.Min (Convert.ToSingle(attribute.value) / Convert.ToSingle(attribute.max), 1.0f);
			}

			//check whether enable status requires change
			if (hideIfValueZero)
			{
				if (attribute.value == 0 && m_value != 0)
				{
					foreach (Transform child in transform)
					{
						child.gameObject.SetActive(false);
					}
				}
				else if (attribute.value != 0 && m_value == 0)
				{
					foreach (Transform child in transform)
					{
						child.gameObject.SetActive(true);
					}
				}
				else
				{
					/* enable already in correct state */
				}
			}

			m_value = attribute.value;
		}
	}
}
