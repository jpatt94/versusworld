using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour
{
	private Slider slider;
	private Text valueText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		slider = GetComponent<Slider>();
		valueText = GetComponentInChildren<Text>();
	}

	/**********************************************************/
	// Callbacks

	public void OnSliderValueChanged()
	{
		valueText.text = Mathf.FloorToInt(slider.value).ToString();
	}

	/**********************************************************/
	// Accessors/Mutators

	public int Value
	{
		get
		{
			return Mathf.FloorToInt(slider.value);
		}
		set
		{
			slider.value = value;
			valueText.text = Mathf.FloorToInt(slider.value).ToString();
		}
	}
}
