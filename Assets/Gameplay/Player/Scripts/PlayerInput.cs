using UnityEngine;
using System.Collections;

public enum ButtonStatus
{
	Down, Pressed, Released,
}

public class PlayerInput : MonoBehaviour
{
	public AnimationCurve sensitivityTranslation;
	public float swapHoldTime;

	private static bool keyboardActive;
	private static bool mouseActive;
	private static Vector2 moveAxis;
	private static Vector2 mouseLookAxis;
	private static float _swapHoldTime;
	private static AnimationCurve _sensitivityTranslation;
	private static float mouseLookSensitivity;
	private static float swapHoldCurrentTime;
	private static bool needsSwapRelease;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Start()
	{
		Reset();

		_swapHoldTime = swapHoldTime;
		_sensitivityTranslation = sensitivityTranslation;
		swapHoldCurrentTime = 0.0f;
	}

	public void Update()
	{
		Reset();

		moveAxis.x = (CheckButton(ControlSettings.MoveLeftKeybind, ButtonStatus.Down) ? -1 : 0) + (CheckButton(ControlSettings.MoveRightKeybind, ButtonStatus.Down) ? 1 : 0);
		moveAxis.y = (CheckButton(ControlSettings.MoveBackKeybind, ButtonStatus.Down) ? -1 : 0) + (CheckButton(ControlSettings.MoveForwardKeybind, ButtonStatus.Down) ? 1 : 0);
		if (moveAxis.sqrMagnitude > 1.0f)
		{
			moveAxis.Normalize();
		}

		mouseLookAxis.x = Input.GetAxisRaw("Mouse X");
		mouseLookAxis.y = Input.GetAxisRaw("Mouse Y");

		if (CheckButton(ControlSettings.SwapKeybind, ButtonStatus.Down))
		{
			swapHoldCurrentTime += Time.deltaTime;
		}

		Cursor.visible = !mouseActive;
		Cursor.lockState = mouseActive ? CursorLockMode.Locked : CursorLockMode.None;
	}

	public void LateUpdate()
	{
		if (!CheckButton(ControlSettings.SwapKeybind, ButtonStatus.Down))
		{
			swapHoldCurrentTime = 0.0f;
			needsSwapRelease = false;
		}
	}

	/**********************************************************/
	// Helper Functions

	private void Reset()
	{
		moveAxis = Vector2.zero;
		mouseLookAxis = Vector2.zero;
	}

	private static bool CheckButton(string name, ButtonStatus status)
	{
		if (!Enabled)
		{
			return false;
		}

		switch (status)
		{
			case ButtonStatus.Down: return Input.GetButton(name);
			case ButtonStatus.Pressed: return Input.GetButtonDown(name);
			case ButtonStatus.Released: return Input.GetButtonUp(name);
		}

		return false;
	}

	private static bool CheckButton(KeyCode key, ButtonStatus status)
	{
		if (!Enabled)
		{
			return false;
		}

		switch (status)
		{
			case ButtonStatus.Down: return Input.GetKey(key);
			case ButtonStatus.Pressed: return Input.GetKeyDown(key);
			case ButtonStatus.Released: return Input.GetKeyUp(key);
		}

		return false;
	}

	private static bool CheckButton(Keybind keybind, ButtonStatus status)
	{
		if (!Enabled)
		{
			return false;
		}

		if (keybind.Key != KeyCode.None)
		{
			switch (status)
			{
				case ButtonStatus.Down: return Input.GetKey(keybind.Key);
				case ButtonStatus.Pressed: return Input.GetKeyDown(keybind.Key);
				case ButtonStatus.Released: return Input.GetKeyUp(keybind.Key);
			}
		}
		else if (keybind.MouseButton > -1)
		{
			switch (status)
			{
				case ButtonStatus.Down: return Input.GetMouseButton(keybind.MouseButton);
				case ButtonStatus.Pressed: return Input.GetMouseButtonDown(keybind.MouseButton);
				case ButtonStatus.Released: return Input.GetMouseButtonUp(keybind.MouseButton);
			}
		}
		else if (keybind.MouseWheel != 0)
		{
			if (keybind.MouseWheel < 0)
			{
				return Input.mouseScrollDelta.y > 0.0f;
			}
			else if (keybind.MouseWheel > 0)
			{
				return Input.mouseScrollDelta.y < 0.0f;
			}
		}

		return false;
	}

	/**********************************************************/
	// Interface

	public static bool Enabled
	{
		get
		{
			return keyboardActive && mouseActive;
		}
		set
		{
			keyboardActive = value;
			mouseActive = value;
		}
	}

	public static bool KeyboardActive
	{
		get
		{
			return keyboardActive;
		}
		set
		{
			keyboardActive = value;
		}
	}

	public static bool MouseActive
	{
		get
		{
			return mouseActive;
		}
		set
		{
			mouseActive = value;
		}
	}

	public static Vector2 MoveAxis
	{
		get
		{
			return keyboardActive ? moveAxis : Vector2.zero;
		}
	}

	public static Vector2 MouseLookAxis
	{
		get
		{
			return mouseActive ? mouseLookAxis * mouseLookSensitivity : Vector2.zero;
		}
	}

	public static float MouseLookSensitivity
	{
		set
		{
			mouseLookSensitivity = _sensitivityTranslation.Evaluate(value);
		}
	}

	public static bool Jump(ButtonStatus status)
	{
		return CheckButton(ControlSettings.JumpKeybind, status);
	}

	public static bool Shoot(ButtonStatus status)
	{
		return CheckButton(ControlSettings.ShootKeybind, status);
	}

	public static bool AimDownSights(ButtonStatus status)
	{
		return CheckButton(ControlSettings.AimDownSightsKeybind, status);
	}

	public static bool Reload(ButtonStatus status)
	{
		return CheckButton(ControlSettings.ReloadKeybind, status);
	}

	public static bool Sprint(ButtonStatus status)
	{
		return CheckButton(ControlSettings.SprintKeybind, status);
	}

	public static bool Swap()
	{
		return CheckButton(ControlSettings.SwapKeybind, ButtonStatus.Released) && !needsSwapRelease;
	}

	public static bool PickUp()
	{
		return swapHoldCurrentTime >= _swapHoldTime && !needsSwapRelease;
	}

	public static void NeedsSwapRelease()
	{
		needsSwapRelease = true;
	}

	public static bool Interact()
	{
		return CheckButton(ControlSettings.SwapKeybind, ButtonStatus.Pressed) && !needsSwapRelease;
	}

	public static bool Throw(ButtonStatus status)
	{
		return CheckButton(ControlSettings.ThrowGrenadeKeybind, status);
	}

	public static bool NextGrenade()
	{
		return CheckButton(ControlSettings.SelectGrenadeDownKeybind, ButtonStatus.Pressed);
	}

	public static bool PreviousGrenade()
	{
		return CheckButton(ControlSettings.SelectGrenadeUpKeybind, ButtonStatus.Pressed);
	}

	public static bool Melee(ButtonStatus status)
	{
		return CheckButton(ControlSettings.MeleeKeybind, status);
	}

	public static bool Crouch(ButtonStatus status)
	{
		return CheckButton(ControlSettings.CrouchKeybind, status);
	}

	public static bool UsePowerUp(ButtonStatus status)
	{
		return CheckButton(ControlSettings.UseItemKeybind, status);
	}

	public static bool ShowScoreboard(ButtonStatus status)
	{
		return CheckButton(ControlSettings.ShowScoreboardKeybind, status);
	}
}
