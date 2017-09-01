using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveNotificationMenu : StackedMenuState
{
	private Text messageText;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		messageText = transform.Find("Panel/MessageText").GetComponent<Text>();
	}

	public override void Start()
	{
	}

	/**********************************************************/
	// Accessors/Mutators

	public string Message
	{
		get
		{
			return messageText.text;
		}
		set
		{
			messageText.text = value;
		}
	}
}
