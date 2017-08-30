using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindControl : MonoBehaviour
{
	[SerializeField]
	private GameObject UIShieldPrefab;
	[SerializeField]
	private Color pressAKeyColor;
	[SerializeField]
	private float pressKeyDuration;

	private float pressKeyTime;
	private bool bufferFrame;

	private SettingsMenu settingsMenu;
	private Text label;
	private Keybind keybind;
	private GameObject uiShield;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		settingsMenu = GetComponentInParent<SettingsMenu>();
		label = GetComponentInChildren<Text>();
	}

	public void Update()
	{
		if (pressKeyTime > 0.0f)
		{
			label.text = "Press a key (" + Mathf.CeilToInt(pressKeyTime).ToString() + ")";
		}

		if (pressKeyTime > 0.0f && !bufferFrame)
		{
			bool pressedNewKey = false;
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				pressedNewKey = true;
			}

			if (!pressedNewKey)
			{
				for (int i = 1; i < (int)KeyCode.Menu; i++)
				{
					if (Input.GetKeyDown((KeyCode)i))
					{
						keybind.Key = (KeyCode)i;
						pressedNewKey = true;
						break;
					}
				}
			}

			if (!pressedNewKey)
			{
				for (int i = 0; i < 6; i++)
				{
					if (Input.GetMouseButtonDown(i))
					{
						keybind.MouseButton = i;
						pressedNewKey = true;
						break;
					}
				}
			}

			if (!pressedNewKey)
			{
				if (Input.mouseScrollDelta.y < 0.0f)
				{
					keybind.MouseWheel = 1;
					pressedNewKey = true;
				}
				else if (Input.mouseScrollDelta.y > 0.0f)
				{
					keybind.MouseWheel = -1;
					pressedNewKey = true;
				}
			}

			pressKeyTime -= Time.deltaTime;
			if (pressKeyTime <= 0.0f || pressedNewKey)
			{
				Destroy(uiShield);
				uiShield = null;
				label.text = keybind.ToString();
				label.color = Color.black;
				pressKeyTime = -1.0f;
			}
		}

		bufferFrame = false;
	}

	/**********************************************************/
	// Interface

	public void OnClick()
	{
		uiShield = Instantiate(UIShieldPrefab);
		uiShield.transform.SetParent(settingsMenu.transform);
		uiShield.transform.localPosition = Vector3.zero;

		label.color = pressAKeyColor;
		pressKeyTime = pressKeyDuration;
		bufferFrame = true;
	}

	/**********************************************************/
	// Accessors/Mutators

	public Keybind Keybind
	{
		get
		{
			return keybind;
		}
		set
		{
			keybind = value;
			label.text = keybind.ToString();
		}
	}
}
