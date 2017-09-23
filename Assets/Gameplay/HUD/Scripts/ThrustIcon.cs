using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrustIcon : AbilityIcon
{
	[SerializeField]
	private float dimAlpha;

	private Image iconImage;
	private Text labelText;
	private Image rechargeImage;
	private Text freeThrustsText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		iconImage = GetComponent<Image>();
		labelText = transform.Find("Label").GetComponent<Text>();
		rechargeImage = transform.Find("RechargeImage").GetComponent<Image>();
		freeThrustsText = transform.Find("FreeThrustsText").GetComponent<Text>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public float RechargeAmount
	{
		get
		{
			return rechargeImage.fillAmount;
		}
		set
		{
			if ((rechargeImage.fillAmount > 0.0f && value == 0.0f) || (rechargeImage.fillAmount == 0.0f && value > 0.0f))
			{
				Pop();
			}

			rechargeImage.fillAmount = value;

			float alpha = value > 0.0f ? dimAlpha : 1.0f;
			Utility.SetAlpha(iconImage, alpha);
			Utility.SetAlpha(labelText, alpha);
		}
	}

	public int FreeThrusts
	{
		set
		{
			if (value > 0)
			{
				freeThrustsText.enabled = true;
				freeThrustsText.text = value.ToString();
			}
			else
			{
				freeThrustsText.enabled = false;
			}
		}
	}
}