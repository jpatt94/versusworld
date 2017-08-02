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

		moveAxis.x = Input.GetAxisRaw("Horizontal");
		moveAxis.y = Input.GetAxisRaw("Vertical");
		if (moveAxis.sqrMagnitude > 1.0f)
		{
			moveAxis.Normalize();
		}

		mouseLookAxis.x = Input.GetAxisRaw("Mouse X");
		mouseLookAxis.y = Input.GetAxisRaw("Mouse Y");

		if (CheckButton("Swap", ButtonStatus.Down))
		{
			swapHoldCurrentTime += Time.deltaTime;
		}

		Cursor.visible = !mouseActive;
		Cursor.lockState = mouseActive ? CursorLockMode.Locked : CursorLockMode.None;
	}

	public void LateUpdate()
	{
		if (!CheckButton("Swap", ButtonStatus.Down))
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
		return CheckButton("Jump", status);
	}

	public static bool Shoot(ButtonStatus status)
	{
		return CheckButton("Shoot", status);
	}

	public static bool AimDownSights(ButtonStatus status)
	{
		return CheckButton("AimDownSights", status);
	}

	public static bool Reload(ButtonStatus status)
	{
		return CheckButton("Reload", status);
	}

	public static bool Sprint(ButtonStatus status)
	{
		return CheckButton("Sprint", status);
	}

	public static bool Swap()
	{
		return CheckButton("Swap", ButtonStatus.Released) && !needsSwapRelease;
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
		return CheckButton("Swap", ButtonStatus.Pressed) && !needsSwapRelease;
	}

	public static bool Throw(ButtonStatus status)
	{
		return CheckButton("Throw", status);
	}

	public static bool NextGrenade()
	{
		return Input.mouseScrollDelta.y < 0.0f;
	}

	public static bool PreviousGrenade()
	{
		return Input.mouseScrollDelta.y > 0.0f;
	}

	public static bool Melee(ButtonStatus status)
	{
		return CheckButton("Melee", status);
	}

	public static bool Crouch(ButtonStatus status)
	{
		return CheckButton("Crouch", status);
	}

	public static bool UsePowerUp(ButtonStatus status)
	{
		return CheckButton(KeyCode.E, status);
	}

	public static bool ShowScoreboard(ButtonStatus status)
	{
		return CheckButton("Scoreboard", status);
	}
}
