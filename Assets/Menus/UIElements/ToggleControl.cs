using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleControl : MonoBehaviour
{
	private Toggle toggle;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		toggle = GetComponent<Toggle>();
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
