using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuiltInGameSettings : MonoBehaviour
{
	private static BuiltInGameSettings instance;

	public float respawnTime;
	public HealthSettings health;
	public WeaponSettings weapons;
	public GrenadeSettings grenades;
	public PowerUpSettings powerUps;
	public MeleeSettings melee;

	//public static BuiltInGameSettings Get
	//{
	//	get
	//	{
	//		if (instance == null)
	//		{
	//			GameObject obj = GameObject.Find("MultiplayerManager");
	//			if (obj)
	//			{
	//				instance = obj.GetComponent<BuiltInGameSettings>();
	//			}
	//		}
	//		return instance;
	//	}
	//}
}
