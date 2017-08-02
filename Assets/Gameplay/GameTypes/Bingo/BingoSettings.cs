using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.Networking;

public class BingoSettings : GameSettings
{
	public string TaskName;

	/**********************************************************/
	// Interface

	protected override void SaveGameSpecificSettings(XmlNode parent)
	{
		base.SaveGameSpecificSettings(parent);

		XmlNode n = doc.CreateElement("Bingo");

		SaveValue(n, "TaskName", TaskName);

		parent.AppendChild(n);
	}

	protected override void LoadGameSpecificSettings(XmlNode parent)
	{
		base.LoadGameSpecificSettings(parent);

		XmlNode n = parent.SelectSingleNode("Bingo");

		if (n != null)
		{
			LoadValue(n, "TaskName", ref TaskName);
		}
	}

	protected override void SerializeGameSpecific(NetworkWriter writer)
	{
		base.SerializeGameSpecific(writer);

		writer.Write(TaskName);
	}

	protected override void DeserializeGameSpecific(NetworkReader reader)
	{
		base.DeserializeGameSpecific(reader);

		TaskName = reader.ReadString();
	}
}