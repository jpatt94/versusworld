using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class DeathmatchSettings : GameSettings
{
	/**********************************************************/
	// Interface

	protected override void SaveGameSpecificSettings(XmlNode parent)
	{
		base.SaveGameSpecificSettings(parent);
	}

	protected override void LoadGameSpecificSettings(XmlNode parent)
	{
		base.LoadGameSpecificSettings(parent);
	}

	protected override void SerializeGameSpecific(NetworkWriter writer)
	{
	}

	protected override void DeserializeGameSpecific(NetworkReader reader)
	{
	}
}