using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	[SerializeField]
	protected string displayText;

	/**********************************************************/
	// Interface

	public virtual void OnInteract()
	{

	}

	/**********************************************************/
	// Accessors/Mutators

	public string DisplayText
	{
		get
		{
			return displayText;
		}
		set
		{
			displayText = value;
		}
	}
}