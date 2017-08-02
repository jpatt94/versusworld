using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrustIcon : MonoBehaviour
{
	private Image image;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		image = GetComponent<Image>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		get
		{
			return image.enabled;
		}
		set
		{
			image.enabled = value;
		}
	}

	public float FillAmount
	{
		get
		{
			return image.fillAmount;
		}
		set
		{
			image.fillAmount = value;
		}
	}
}