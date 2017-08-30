using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControlSettingsMenu : SettingsMenuState
{
	private SliderControl mouseSensitivityControl;
	private ToggleControl invertAimControl;
	private ToggleControl toggleCrouchControl;
	private ToggleControl toggleSprintControl;
	private KeybindControl moveForwardControl;
	private KeybindControl moveBackControl;
	private KeybindControl moveLeftControl;
	private KeybindControl moveRightControl;
	private KeybindControl jumpControl;
	private KeybindControl shootControl;
	private KeybindControl reloadControl;
	private KeybindControl aimDownSightsControl;
	private KeybindControl throwGrenadeControl;
	private KeybindControl meleeControl;
	private KeybindControl swapControl;
	private KeybindControl useItemControl;
	private KeybindControl sprintControl;
	private KeybindControl crouchControl;
	private KeybindControl selectGrenadeUpControl;
	private KeybindControl selectGrenadeDownControl;
	private KeybindControl showScoreboardControl;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		mouseSensitivityControl = transform.Find("MouseSensitivityControl").GetComponentInChildren<SliderControl>();
		invertAimControl = transform.Find("InvertAimControl").GetComponentInChildren<ToggleControl>();
		toggleCrouchControl = transform.Find("ToggleCrouchControl").GetComponentInChildren<ToggleControl>();
		toggleSprintControl = transform.Find("ToggleSprintControl").GetComponentInChildren<ToggleControl>();
		moveForwardControl = transform.Find("MoveForwardControl").GetComponentInChildren<KeybindControl>();
		moveBackControl = transform.Find("MoveBackControl").GetComponentInChildren<KeybindControl>();
		moveLeftControl = transform.Find("MoveLeftControl").GetComponentInChildren<KeybindControl>();
		moveRightControl = transform.Find("MoveRightControl").GetComponentInChildren<KeybindControl>();
		jumpControl = transform.Find("JumpControl").GetComponentInChildren<KeybindControl>();
		shootControl = transform.Find("ShootControl").GetComponentInChildren<KeybindControl>();
		reloadControl = transform.Find("ReloadControl").GetComponentInChildren<KeybindControl>();
		aimDownSightsControl = transform.Find("AimDownSightsControl").GetComponentInChildren<KeybindControl>();
		throwGrenadeControl = transform.Find("ThrowGrenadeControl").GetComponentInChildren<KeybindControl>();
		meleeControl = transform.Find("MeleeControl").GetComponentInChildren<KeybindControl>();
		swapControl = transform.Find("SwapControl").GetComponentInChildren<KeybindControl>();
		useItemControl = transform.Find("UseItemControl").GetComponentInChildren<KeybindControl>();
		sprintControl = transform.Find("SprintControl").GetComponentInChildren<KeybindControl>();
		crouchControl = transform.Find("CrouchControl").GetComponentInChildren<KeybindControl>();
		selectGrenadeUpControl = transform.Find("SelectGrenadeUpControl").GetComponentInChildren<KeybindControl>();
		selectGrenadeDownControl = transform.Find("SelectGrenadeDownControl").GetComponentInChildren<KeybindControl>();
		showScoreboardControl = transform.Find("ShowScoreboardControl").GetComponentInChildren<KeybindControl>();

		JP.Event.Register(this, "OnControlSettingsLoad");
		JP.Event.Register(this, "OnMouseSensitivityControlValueChanged");
		JP.Event.Register(this, "OnInvertAimControlValueChanged");
		JP.Event.Register(this, "OnToggleCrouchControlValueChanged");
		JP.Event.Register(this, "OnToggleSprintControlValueChanged");
	}

	/**********************************************************/
	// Interface

	public override void StateBegin()
	{
		base.StateBegin();

		UpdateControlValues();
	}

	public void Save()
	{
		ControlSettings.Save();
	}

	public void OnControlSettingsLoad()
	{
		UpdateControlValues();
	}

	/**********************************************************/
	// Control Callbacks

	public void OnMouseSensitivityControlValueChanged()
	{
		ControlSettings.MouseSensitivity = mouseSensitivityControl.Value;
	}

	public void OnInvertAimControlValueChanged()
	{
		ControlSettings.InvertAim = invertAimControl.Checked;
	}

	public void OnToggleCrouchControlValueChanged()
	{
		ControlSettings.ToggleCrouch = toggleCrouchControl.Checked;
	}

	public void OnToggleSprintControlValueChanged()
	{
		ControlSettings.ToggleSprint = toggleSprintControl.Checked;
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateControlValues()
	{
		mouseSensitivityControl.Value = ControlSettings.MouseSensitivity;
		invertAimControl.Checked = ControlSettings.InvertAim;
		toggleSprintControl.Checked = ControlSettings.ToggleSprint;
		toggleCrouchControl.Checked = ControlSettings.ToggleCrouch;
		moveForwardControl.Keybind = ControlSettings.MoveForwardKeybind;
		moveBackControl.Keybind = ControlSettings.MoveBackKeybind;
		moveLeftControl.Keybind = ControlSettings.MoveLeftKeybind;
		moveRightControl.Keybind = ControlSettings.MoveRightKeybind;
		jumpControl.Keybind = ControlSettings.JumpKeybind;
		shootControl.Keybind = ControlSettings.ShootKeybind;
		reloadControl.Keybind = ControlSettings.ReloadKeybind;
		aimDownSightsControl.Keybind = ControlSettings.AimDownSightsKeybind;
		throwGrenadeControl.Keybind = ControlSettings.ThrowGrenadeKeybind;
		meleeControl.Keybind = ControlSettings.MeleeKeybind;
		swapControl.Keybind = ControlSettings.SwapKeybind;
		useItemControl.Keybind = ControlSettings.UseItemKeybind;
		sprintControl.Keybind = ControlSettings.SprintKeybind;
		crouchControl.Keybind = ControlSettings.CrouchKeybind;
		selectGrenadeUpControl.Keybind = ControlSettings.SelectGrenadeUpKeybind;
		selectGrenadeDownControl.Keybind = ControlSettings.SelectGrenadeDownKeybind;
		showScoreboardControl.Keybind = ControlSettings.ShowScoreboardKeybind;
	}
}
