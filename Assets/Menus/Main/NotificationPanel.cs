using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPanel : MonoBehaviour
{
	private Canvas canvas;
	private Text messageText;
	private Button okButton;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		canvas = GetComponent<Canvas>();
		messageText = Utility.FindChild(gameObject, "MessageText").GetComponent<Text>();
		okButton = Utility.FindChild(gameObject, "OKButton").GetComponent<Button>();
	}

	/**********************************************************/
	// Interface

	public void Show(string message, bool showOkButton)
	{
		//canvas.enabled = true;
		messageText.text = message;
		Utility.EnableButton(okButton, showOkButton);
	}

	public void Hide()
	{
		canvas.enabled = false;
	}

	public void OnOKClick()
	{
		Hide();
	}
}