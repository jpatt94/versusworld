using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
	private string eventName;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		eventName = "On" + gameObject.name + "Click";
	}

	/**********************************************************/
	// Interface

	public void OnClick()
	{
		JP.Event.Trigger(eventName);
	}
}