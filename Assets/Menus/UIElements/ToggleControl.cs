using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleControl : MonoBehaviour
{
	private string eventName;

	private Toggle toggle;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		toggle = GetComponent<Toggle>();
		toggle.onValueChanged.AddListener(OnValueChanged);

		eventName = "On" + transform.parent.name + "ValueChanged";
	}

	/**********************************************************/
	// Callbacks

	public void OnValueChanged(bool value)
	{
		JP.Event.Trigger(eventName);
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Checked
	{
		get
		{
			return toggle.isOn;
		}
		set
		{
			toggle.isOn = value;
		}
	}
}
