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

		Utility.XMLSaveInt(doc, head, "MouseSensitivity", mouseSensitivity);
		Utility.XMLSaveBool(doc, head, "InvertAim", invertAim);
		Utility.XMLSaveBool(doc, head, "ToggleSprint", toggleSprint);
		Utility.XMLSaveBool(doc, head, "ToggleCrouch", toggleCrouch);
		XMLSaveKeybind(doc, head, "MoveForwardKeybind", moveForwardKeybind);
		XMLSaveKeybind(doc, head, "MoveBackKeybind", moveBackKeybind);
		XMLSaveKeybind(doc, head, "MoveLeftKeybind", moveLeftKeybind);
		XMLSaveKeybind(doc, head, "MoveRightKeybind", moveRightKeybind);
		XMLSaveKeybind(doc, head, "JumpKeybind", jumpKeybind);
		XMLSaveKeybind(doc, head, "ShootKeybind", shootKeybind);
		XMLSaveKeybind(doc, head, "ReloadKeybind", reloadKeybind);
		XMLSaveKeybind(doc, head, "AimDownSightsKeybind", aimDownSightsKeybind);
		XMLSaveKeybind(doc, head, "ThrowGrenadeKeybind", throwGrenadeKeybind);
		XMLSaveKeybind(doc, head, "MeleeKeybind", meleeKeybind);
		XMLSaveKeybind(doc, head, "SwapKeybind", swapKeybind);
		XMLSaveKeybind(doc, head, "UseItemKeybind", useItemKeybind);
		XMLSaveKeybind(doc, head, "SprintKeybind", sprintKeybind);
		XMLSaveKeybind(doc, head, "CrouchKeybind", crouchKeybind);
		XMLSaveKeybind(doc, head, "SelectGrenadeUpKeybind", selectGrenadeUpKeybind);
		XMLSaveKeybind(doc, head, "SelectGrenadeDownKeybind", selectGrenadeDownKeybind);
		XMLSaveKeybind(doc, head, "ShowScoreboardKeybind", showScoreboardKeybind);

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

			Utility.XMLLoadInt(head, "MouseSensitivity", out mouseSensitivity);
			Utility.XMLLoadBool(head, "InvertAim", out invertAim);
			Utility.XMLLoadBool(head, "ToggleSprint", out toggleSprint);
			Utility.XMLLoadBool(head, "ToggleCrouch", out toggleCrouch);
			XMLLoadKeybind(head, "MoveForwardKeybind", ref moveForwardKeybind);
			XMLLoadKeybind(head, "MoveBackKeybind", ref moveBackKeybind);
			XMLLoadKeybind(head, "MoveLeftKeybind", ref moveLeftKeybind);
			XMLLoadKeybind(head, "MoveRightKeybind", ref moveRightKeybind);
			XMLLoadKeybind(head, "JumpKeybind", ref jumpKeybind);
			XMLLoadKeybind(head, "ShootKeybind", ref shootKeybind);
			XMLLoadKeybind(head, "ReloadKeybind", ref reloadKeybind);
			XMLLoadKeybind(head, "AimDownSightsKeybind", ref aimDownSightsKeybind);
			XMLLoadKeybind(head, "ThrowGrenadeKeybind", ref throwGrenadeKeybind);
			XMLLoadKeybind(head, "MeleeKeybind", ref meleeKeybind);
			XMLLoadKeybind(head, "SwapKeybind", ref swapKeybind);
			XMLLoadKeybind(head, "UseItemKeybind", ref useItemKeybind);
			XMLLoadKeybind(head, "SprintKeybind", ref sprintKeybind);
			XMLLoadKeybind(head, "CrouchKeybind", ref crouchKeybind);
			XMLLoadKeybind(head, "SelectGrenadeUpKeybind", ref selectGrenadeUpKeybind);
			XMLLoadKeybind(head, "SelectGrenadeDownKeybind", ref selectGrenadeDownKeybind);
			XMLLoadKeybind(head, "ShowScoreboardKeybind", ref showScoreboardKeybind);
		}
		else
		{
			Save();
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

	private static void XMLSaveKeybind(XmlDocument doc, XmlNode parent, string name, Keybind keybind)
	{
		XmlNode node = doc.CreateElement(name);

		XmlAttribute typeAttr = doc.CreateAttribute("Type");
		XmlAttribute valueAttr = doc.CreateAttribute("Value");

		if (keybind.Key != KeyCode.None)
		{
			typeAttr.Value = "KEY";
			valueAttr.Value = ((int)keybind.Key).ToString();
		}
		else if (keybind.MouseButton > -1)
		{
			typeAttr.Value = "MOUSE_BUTTON";
			valueAttr.Value = keybind.MouseButton.ToString();
		}
		else if (keybind.MouseWheel != 0)
		{
			typeAttr.Value = "MOUSE_WHEEL";
			valueAttr.Value = keybind.MouseWheel.ToString();
		}
		else
		{
			typeAttr.Value = "N/A";
			valueAttr.Value = "N/A";
		}

		node.Attributes.Append(typeAttr);
		node.Attributes.Append(valueAttr);

		parent.AppendChild(node);
	}

	private static void XMLLoadKeybind(XmlNode parent, string name, ref Keybind keybind)
	{
		XmlNode node = parent.SelectSingleNode(name);
		string type = node.Attributes["Type"].Value;
		int value = System.Convert.ToInt32(node.Attributes["Value"].Value);

		if (type == "KEY")
		{
			keybind.Key = (KeyCode)value;
		}
		else if (type == "MOUSE_BUTTON")
		{
			keybind.MouseButton = value;
		}
		else if (type == "MOUSE_WHEEL")
		{
			keybind.MouseWheel = value;
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
