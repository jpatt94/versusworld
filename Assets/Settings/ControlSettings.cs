using UnityEngine;
using System.Collections;
using System.Xml;

public class ControlSettings : MonoBehaviour
{
	private static int mouseSensitivity;
	private static bool invertAim;
	private static bool toggleSprint;
	private static bool toggleCrouch;
	private static Keybind moveForwardKeybind;
	private static Keybind moveBackKeybind;
	private static Keybind moveLeftKeybind;
	private static Keybind moveRightKeybind;
	private static Keybind jumpKeybind;
	private static Keybind shootKeybind;
	private static Keybind reloadKeybind;
	private static Keybind aimDownSightsKeybind;
	private static Keybind throwGrenadeKeybind;
	private static Keybind meleeKeybind;
	private static Keybind swapKeybind;
	private static Keybind useItemKeybind;
	private static Keybind sprintKeybind;
	private static Keybind crouchKeybind;
	private static Keybind selectGrenadeUpKeybind;
	private static Keybind selectGrenadeDownKeybind;
	private static Keybind showScoreboardKeybind;

	/**********************************************************/
	// Interface

	public static void SetDefaults()
	{
		mouseSensitivity = 50;
		invertAim = false;
		toggleSprint = true;
		toggleCrouch = true;

		SafeInitializeKeybind(ref moveForwardKeybind);
		SafeInitializeKeybind(ref moveBackKeybind);
		SafeInitializeKeybind(ref moveLeftKeybind);
		SafeInitializeKeybind(ref moveRightKeybind);
		SafeInitializeKeybind(ref jumpKeybind);
		SafeInitializeKeybind(ref shootKeybind);
		SafeInitializeKeybind(ref reloadKeybind);
		SafeInitializeKeybind(ref aimDownSightsKeybind);
		SafeInitializeKeybind(ref throwGrenadeKeybind);
		SafeInitializeKeybind(ref meleeKeybind);
		SafeInitializeKeybind(ref swapKeybind);
		SafeInitializeKeybind(ref useItemKeybind);
		SafeInitializeKeybind(ref sprintKeybind);
		SafeInitializeKeybind(ref crouchKeybind);
		SafeInitializeKeybind(ref selectGrenadeUpKeybind);
		SafeInitializeKeybind(ref selectGrenadeDownKeybind);
		SafeInitializeKeybind(ref showScoreboardKeybind);

		moveForwardKeybind.Key = KeyCode.W;
		moveBackKeybind.Key = KeyCode.S;
		moveLeftKeybind.Key = KeyCode.A;
		moveRightKeybind.Key = KeyCode.D;
		jumpKeybind.Key = KeyCode.Space;
		shootKeybind.MouseButton = 0;
		reloadKeybind.Key = KeyCode.R;
		aimDownSightsKeybind.MouseButton = 1;
		throwGrenadeKeybind.Key = KeyCode.G;
		meleeKeybind.Key = KeyCode.F;
		swapKeybind.Key = KeyCode.Q;
		useItemKeybind.Key = KeyCode.E;
		sprintKeybind.Key = KeyCode.LeftShift;
		crouchKeybind.Key = KeyCode.LeftControl;
		selectGrenadeUpKeybind.MouseWheel = -1;
		selectGrenadeDownKeybind.MouseWheel = 1;
		showScoreboardKeybind.Key = KeyCode.Tab;
	}

	public static void Save()
	{
		string fileName = Utility.GetSettingsDirectory();
		System.IO.Directory.CreateDirectory(fileName);
		fileName += "/ControlSettings.xml";

		XmlDocument doc = new XmlDocument();

		XmlNode head = doc.CreateElement("ControlSettings");
		doc.AppendChild(head);

		XmlNode ms = doc.CreateElement("MouseSensitivity");
		XmlAttribute attr = doc.CreateAttribute("Value");
		attr.Value = mouseSensitivity.ToString();
		ms.Attributes.Append(attr);
		head.AppendChild(ms);

		doc.Save(fileName);
	}

	public static void Load()
	{
		SetDefaults();

		string fileName = Utility.GetSettingsDirectory() + "/ControlSettings.xml";
		if (System.IO.File.Exists(fileName))
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);

			XmlNode head = doc.SelectSingleNode("ControlSettings");

			XmlNode ms = head.SelectSingleNode("MouseSensitivity");
			mouseSensitivity = System.Convert.ToInt32(ms.Attributes["Value"].Value);
		}

		JP.Event.Trigger("OnControlSettingsLoad");
	}

	/**********************************************************/
	// Helper Functions

	private static void SafeInitializeKeybind(ref Keybind keybind)
	{
		if (keybind == null)
		{
			keybind = new Keybind();
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public static int MouseSensitivity
	{
		get
		{
			return mouseSensitivity;
		}
		set
		{
			mouseSensitivity = value;
		}
	}

	public static bool InvertAim
	{
		get
		{
			return invertAim;
		}
		set
		{
			invertAim = value;
		}
	}

	public static bool ToggleSprint
	{
		get
		{
			return toggleSprint;
		}
		set
		{
			toggleSprint = value;
		}
	}

	public static bool ToggleCrouch
	{
		get
		{
			return toggleCrouch;
		}
		set
		{
			toggleCrouch = value;
		}
	}

	public static Keybind MoveForwardKeybind
	{
		get
		{
			return moveForwardKeybind;
		}
	}

	public static Keybind MoveBackKeybind
	{
		get
		{
			return moveBackKeybind;
		}
	}

	public static Keybind MoveLeftKeybind
	{
		get
		{
			return moveLeftKeybind;
		}
	}

	public static Keybind MoveRightKeybind
	{
		get
		{
			return moveRightKeybind;
		}
	}

	public static Keybind JumpKeybind
	{
		get
		{
			return jumpKeybind;
		}
	}

	public static Keybind ShootKeybind
	{
		get
		{
			return shootKeybind;
		}
	}

	public static Keybind ReloadKeybind
	{
		get
		{
			return reloadKeybind;
		}
	}

	public static Keybind AimDownSightsKeybind
	{
		get
		{
			return aimDownSightsKeybind;
		}
	}

	public static Keybind ThrowGrenadeKeybind
	{
		get
		{
			return throwGrenadeKeybind;
		}
	}

	public static Keybind MeleeKeybind
	{
		get
		{
			return meleeKeybind;
		}
	}

	public static Keybind SwapKeybind
	{
		get
		{
			return swapKeybind;
		}
	}

	public static Keybind UseItemKeybind
	{
		get
		{
			return useItemKeybind;
		}
	}

	public static Keybind SprintKeybind
	{
		get
		{
			return sprintKeybind;
		}
	}

	public static Keybind CrouchKeybind
	{
		get
		{
			return crouchKeybind;
		}
	}

	public static Keybind SelectGrenadeUpKeybind
	{
		get
		{
			return selectGrenadeUpKeybind;
		}
	}

	public static Keybind SelectGrenadeDownKeybind
	{
		get
		{
			return selectGrenadeDownKeybind;
		}
	}

	public static Keybind ShowScoreboardKeybind
	{
		get
		{
			return showScoreboardKeybind;
		}
	}
}
