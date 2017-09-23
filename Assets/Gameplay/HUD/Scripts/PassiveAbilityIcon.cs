using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveAbilityIcon : AbilityIcon
{
	private Image iconImage;
	private Text labelText;
	private Image timerImage;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		iconImage = GetComponent<Image>();
		labelText = transform.Find("Label").GetComponent<Text>();
		timerImage = transform.Find("TimerImage").GetComponent<Image>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public float TimerAmount
	{
		get
		{
			return timerImage.fillAmount;
		}
		set
		{
			timerImage.fillAmount = value;
		}
	}
}
