using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class SiegeSettings : GameSettings
{
	public int ScorePerCaptureSuccess;
	public int ScorePerCaptureFailure;
	public int CaptureDuration;
	public float MultipleCapturersSpeed;

	/**********************************************************/
	// Interface

	protected override void SaveGameSpecificSettings(XmlNode parent)
	{
		base.SaveGameSpecificSettings(parent);

		XmlNode n = doc.CreateElement("Siege");

		SaveValue(n, "ScorePerCaptureSuccess", ScorePerCaptureSuccess);
		SaveValue(n, "ScorePerCaptureFailure", ScorePerCaptureFailure);
		SaveValue(n, "CaptureDuration", CaptureDuration);
		SaveValue(n, "MultipleCapturersSpeed", MultipleCapturersSpeed);

		parent.AppendChild(n);
	}

	protected override void LoadGameSpecificSettings(XmlNode parent)
	{
		base.LoadGameSpecificSettings(parent);

		XmlNode n = parent.SelectSingleNode("Siege");

		if (n != null)
		{
			LoadValue(n, "ScorePerCaptureSuccess", ref ScorePerCaptureSuccess);
			LoadValue(n, "ScorePerCaptureFailure", ref ScorePerCaptureFailure);
			LoadValue(n, "CaptureDuration", ref CaptureDuration);
			LoadValue(n, "MultipleCapturersSpeed", ref MultipleCapturersSpeed);
		}
	}

	protected override void SerializeGameSpecific(NetworkWriter writer)
	{
		base.SerializeGameSpecific(writer);

		writer.Write((short)ScorePerCaptureSuccess);
		writer.Write((short)ScorePerCaptureFailure);
		writer.Write((short)CaptureDuration);
		writer.Write(MultipleCapturersSpeed);
	}

	protected override void DeserializeGameSpecific(NetworkReader reader)
	{
		base.DeserializeGameSpecific(reader);

		ScorePerCaptureSuccess = reader.ReadInt16();
		ScorePerCaptureFailure = reader.ReadInt16();
		CaptureDuration = reader.ReadInt16();
		MultipleCapturersSpeed = reader.ReadSingle();
	}
}