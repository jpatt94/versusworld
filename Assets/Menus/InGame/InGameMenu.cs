using UnityEngine;
using System.Collections;

public class InGameMenu : MonoBehaviour
{
	private Canvas canvas;
	private SettingsMenu settingsMenu;
	private CanvasRenderer[] canvasRenderers;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		canvas = GetComponent<Canvas>();
		settingsMenu = GetComponentInChildren<SettingsMenu>();

		JP.Event.Register(this, "OnResumeButtonClick");
		JP.Event.Register(this, "OnInGameSettingsButtonClick");
		JP.Event.Register(this, "OnLeaveGameButtonClick");
		JP.Event.Register(this, "OnExitToWindowsButtonClick");

		canvasRenderers = transform.Find("Panel").GetComponentsInChildren<CanvasRenderer>();
	}

	public void Start()
	{
		Enable(false);
		settingsMenu.Visible = false;
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !settingsMenu.Visible)
		{
			Enable(!canvas.enabled);
		}
	}

	public void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
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

	/**********************************************************/
	// Button Callbacks

	public void OnResumeButtonClick()
	{
		Enable(false);
	}

	public void OnInGameSettingsButtonClick()
	{
		Visible = false;

		settingsMenu.Visible = true;
		settingsMenu.StateBegin();
	}

	public void OnLeaveGameButtonClick()
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

	public void OnExitToWindowsButtonClick()
	{
		Application.Quit();
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			foreach (CanvasRenderer r in canvasRenderers)
			{
				r.gameObject.SetActive(value);
			}
		}
	}
}
