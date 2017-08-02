using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameSettingsMessage : MessageBase
{
	public GameType Type;
	public byte[] Settings;
}