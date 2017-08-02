using UnityEngine;
using System.Collections;

public class InGameMenu : MonoBehaviour
{
	private Canvas canvas;
	private InGameSettingsMenu settingsMenu;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		canvas = GetComponent<Canvas>();
		settingsMenu = GameObject.Find("SettingsMenu").GetComponent<InGameSettingsMenu>();
	}

	public void Start()
	{
		Enable(false);
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Enable(!canvas.enabled);
		}
	}

	/**********************************************************/
	// Interface

	public void Enable(bool enable)
	{
		canvas.enabled = enable;
		PlayerInput.Enabled = !enable;
		Cursor.visible = canvas.enabled;
		Cursor.lockState = canvas.enabled ? CursorLockMode.None : CursorLockMode.Locked;
	}

	public void OnResumeClick()
	{
		Enable(false);
	}

	public void OnSettingsClick()
	{
		canvas.enabled = false;
		settingsMenu.Enable(true);
	}

	public void OnLeaveGameClick()
	{
		Enable(false);

		PartyManager party = FindObjectOfType<PartyManager>();
		if (party.isServer)
		{
			party.Game.EndGame();
		}
		else
		{
			party.ReturnToMainMenu();
		}
	}

	public void OnExitClick()
	{
		Application.Quit();
	}
}
