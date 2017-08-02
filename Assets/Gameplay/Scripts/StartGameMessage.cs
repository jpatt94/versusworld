using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StartGameMessage : MessageBase
{
	public string MapName;
	public GameType GameType;
	public byte[] GameSettings;
}