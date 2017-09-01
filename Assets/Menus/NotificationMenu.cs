using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationMenu : StackedMenuState
{
	private Text messageText;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		messageText = transform.Find("Panel/MessageText").GetComponent<Text>();

		JP.Event.Register(this, "OnOKButtonClick");
	}

	public override void Start()
	{
	}

	public override void Update()
	{
		base.Update();

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			OnOKButtonClick();
		}
	}

	/**********************************************************/
	// Button Callbacks

	public void OnOKButtonClick()
	{
		StateEnd();
		Destroy(gameObject);
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
