using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour
{
	private string eventName;

	private Slider slider;
	private Text valueText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		slider = GetComponent<Slider>();
		slider.onValueChanged.AddListener(OnSliderValueChanged);
		valueText = GetComponentInChildren<Text>();

		eventName = "On" + transform.parent.name + "ValueChanged";
	}

	/**********************************************************/
	// Callbacks

	public void OnSliderValueChanged(float value)
	{
		valueText.text = Mathf.FloorToInt(slider.value).ToString();
		JP.Event.Trigger(eventName);
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
