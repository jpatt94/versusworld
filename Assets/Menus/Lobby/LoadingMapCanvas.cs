using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMapCanvas : MonoBehaviour
{
	[SerializeField]
	private Sprite[] mapBackgrounds;
	[SerializeField]
	private string[] progressMessage;
	[SerializeField]
	private float[] progressPercent;

	private int progress;
	private float progressBarWidth;

	private Image backgroundImage;
	private Text nameText;
	private Text progressText;
	private Image progressBar;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		DontDestroyOnLoad(gameObject);

		backgroundImage = transform.Find("Background").GetComponent<Image>();
		nameText = transform.Find("NameText").GetComponent<Text>();
		progressText = transform.Find("Panel/ProgressText").GetComponent<Text>();
		progressBar = transform.Find("ProgressBarBackground/ProgressBar").GetComponent<Image>();

		progressBarWidth = progressBar.rectTransform.offsetMax.x;
	}

	/**********************************************************/
	// Accessors/Mutators

	public int MapIndex
	{
		set
		{
			backgroundImage.sprite = mapBackgrounds[value];
		}
	}

	public string MapName
	{
		set
		{
			nameText.text = value;
		}
	}

	public void IncreaseProgress()
	{
		if (progress >= progressMessage.Length || progress >= progressPercent.Length)
		{
			progressText.text = "Invalid index: " + progress;
			return;
		}

		progressText.text = progressMessage[progress];

		Vector2 offset = progressBar.rectTransform.offsetMax;
		offset.x = progressBarWidth * (1.0f - progressPercent[progress]);
		progressBar.rectTransform.offsetMax = offset;

		progress++;
	}
}