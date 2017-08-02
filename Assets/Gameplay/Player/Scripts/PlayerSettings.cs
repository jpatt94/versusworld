using UnityEngine;
using System.Collections;
using System.Xml;
using System;

namespace JP
{
	public class PlayerSettings : MonoBehaviour
	{
		private static int mouseSensitivity;

		/**********************************************************/
		// Interface

		public static void InitializeDefaults()
		{
			mouseSensitivity = 50;
		}

		public static void LoadFromFile(string fileName)
		{
			XmlDocument xml = new XmlDocument();
			xml.Load(fileName);


		}

		public static void LoadFromAppData()
		{
			string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ZeroMinusRed/PlayerSettings.xml";
			LoadFromFile(fileName);
		}

		public static void SaveToFile(string fileName)
		{

			XmlDocument xml = new XmlDocument();
			XmlNode head = xml.CreateElement("PlayerSettings");
			xml.AppendChild(head);

			XmlNode ms = xml.CreateElement("MouseSensitivity");
			ms.InnerText = mouseSensitivity.ToString();
			head.AppendChild(ms);

			xml.Save(fileName);
		}

		public static void SaveToAppData()
		{
			string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ZeroMinusRed";
			System.IO.Directory.CreateDirectory(fileName);

			fileName += "/PlayerSettings.xml";
			SaveToFile(fileName);
		}

		/**********************************************************/
		// Accessors/Mutators

		public int MouseSensitivity
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
}
