using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindControl : MonoBehaviour
{
	private Text label;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		label = GetComponentInChildren<Text>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public string Label
	{
		get
		{
			return label.text;
		}
		set
		{
			label.text = value;
		}
	}
}
