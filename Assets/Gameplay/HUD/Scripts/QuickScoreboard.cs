using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickScoreboard : MonoBehaviour
{
	private float progressBarZero;
	private int maxScore;

	private Image topBackground;
	private Image topProgressBar;
	private Text topText;
	private Image topArrow;
	private Image bottomBackground;
	private Image bottomProgressBar;
	private Text bottomText;
	private Image bottomArrow;
	private Text gameModeText;
	private Text timeText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		topBackground = Utility.FindChild(gameObject, "TopBarBackground").GetComponent<Image>();
		topProgressBar = Utility.FindChild(gameObject, "TopBarProgress").GetComponent<Image>();
		topText = Utility.FindChild(gameObject, "TopText").GetComponent<Text>();
		topArrow = Utility.FindChild(gameObject, "TopArrow").GetComponent<Image>();
		bottomBackground = Utility.FindChild(gameObject, "BottomBarBackground").GetComponent<Image>();
		bottomProgressBar = Utility.FindChild(gameObject, "BottomBarProgress").GetComponent<Image>();
		bottomText = Utility.FindChild(gameObject, "BottomText").GetComponent<Text>();
		bottomArrow = Utility.FindChild(gameObject, "BottomArrow").GetComponent<Image>();
		gameModeText = transform.Find("GameModeText").GetComponent<Text>();
		timeText = transform.Find("TimeText").GetComponent<Text>();

		progressBarZero = topProgressBar.rectTransform.offsetMax.x;
	}

	/**********************************************************/
	// Interface

	public void SetScores(int localScore, int otherScore, Color localColor, Color otherColor)
	{
		bool localOnTop = localScore >= otherScore;

		topText.text = localOnTop ? localScore.ToString() : otherScore.ToString();
		bottomText.text = localOnTop ? otherScore.ToString() : localScore.ToString();

		topArrow.enabled = localOnTop;
		bottomArrow.enabled = !localOnTop;

		topProgressBar.enabled = localOnTop ? (localScore > 0) : (otherScore > 0);
		bottomProgressBar.enabled = localOnTop ? (otherScore > 0) : (localScore > 0);

		topProgressBar.rectTransform.offsetMax = new Vector2(Mathf.Lerp(progressBarZero, 0.0f, (localOnTop ? localScore : otherScore) / (float)maxScore), topProgressBar.rectTransform.offsetMax.y);
		bottomProgressBar.rectTransform.offsetMax = new Vector2(Mathf.Lerp(progressBarZero, 0.0f, (localOnTop ? otherScore : localScore) / (float)maxScore), bottomProgressBar.rectTransform.offsetMax.y);

		Utility.SetRGB(topBackground, localOnTop ? localColor : otherColor);
		Utility.SetRGB(bottomBackground, localOnTop ? otherColor : localColor);
	}

	/**********************************************************/
	// Accessors/Mutators

	public int MaxScore
	{
		get
		{
			return maxScore;
		}
		set
		{
			maxScore = value;
		}
	}

	public string GameModeText
	{
		get
		{
			return gameModeText.text;
		}
		set
		{
			gameModeText.text = value;
		}
	}

	public string TimeText
	{
		get
		{
			return timeText.text;
		}
		set
		{
			timeText.text = value;
		}
	}
}
