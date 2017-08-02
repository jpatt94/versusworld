using UnityEngine;
using System.Collections;
using System.Xml;

public class ControlSettings : MonoBehaviour
{
	private static int mouseSensitivity;

	/**********************************************************/
	// Interface

	public static void SetDefaults()
	{
		mouseSensitivity = 50;
	}

	public static void Load()
	{
		SetDefaults();

		string fileName = Utility.GetSettingsDirectory() + "/ControlSettings.xml";
		if (System.IO.File.Exists(fileName))
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);

			XmlNode head = doc.SelectSingleNode("//ControlSettings");

			XmlNode ms = head.SelectSingleNode("//MouseSensitivity");
			mouseSensitivity = System.Convert.ToInt32(ms.Attributes["Value"].Value);
		}

		JP.Event.Trigger("OnControlSettingsLoad");
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
}
