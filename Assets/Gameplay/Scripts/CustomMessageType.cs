using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomMessageType
{
	public const int PartyRejection = MsgType.Highest + 1;
	public const int GameSettings = MsgType.Highest + 2;
	public const int StartGame = MsgType.Highest + 3;
	public const int PlayerStats = MsgType.Highest + 4;
}