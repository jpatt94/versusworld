using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControlSettingsMenu : MonoBehaviour
{
	private Slider mouseSensSlider;
	private Text mouseSensValueText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		GameObject obj = Utility.FindChild(gameObject, "MouseSensitivitySlider");
		mouseSensSlider = obj.GetComponentInChildren<Slider>();
		mouseSensValueText = Utility.FindChild(obj, "Text").GetComponent<Text>();

		JP.Event.Register(this, "OnControlSettingsLoad");
	}

	public void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public void Save()
	{
		ControlSettings.MouseSensitivity = System.Convert.ToInt32(mouseSensValueText.text);

		ControlSettings.Save();
	}

	public void OnControlSettingsLoad()
	{
		mouseSensValueText.text = ControlSettings.MouseSensitivity.ToString();
		mouseSensSlider.value = ControlSettings.MouseSensitivity;
	}

	public void OnMouseSensitivitySliderDrag()
	{
		mouseSensValueText.text = ((int)mouseSensSlider.value).ToString();
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			CanvasRenderer[] rs = GetComponentsInChildren<CanvasRenderer>();
			foreach (CanvasRenderer r in rs)
			{
				r.cull = !value;
			}
		}
	}
}
