using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
	private Image fadeImage;
	private Image messagePanel;
	private Text gameOverText;
	private Text winnerText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		fadeImage = GetComponent<Image>();
		messagePanel = transform.Find("MessagePanel").GetComponent<Image>();
		gameOverText = messagePanel.transform.Find("GameOverText").GetComponent<Text>();
		winnerText = messagePanel.transform.Find("WinnerText").GetComponent<Text>();

		Visible = false;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		get
		{
			return fadeImage.enabled;
		}
		set
		{
			fadeImage.enabled = value;
			messagePanel.enabled = value;
			gameOverText.enabled = value;
			winnerText.enabled = value;
		}
	}

	public float FadeAlpha
	{
		get
		{
			return fadeImage.color.a;
		}
		set
		{
			Utility.SetAlpha(fadeImage, value);
		}
	}

	public string WinnerText
	{
		get
		{
			return winnerText.text;
		}
		set
		{
			winnerText.text = value;
		}
	}
}