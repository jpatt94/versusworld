using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWaypoint : Waypoint
{
	private Image icon;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		icon = transform.Find("Icon").GetComponent<Image>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public WeaponType WeaponType
	{
		set
		{
			icon.sprite = WeaponManager.GetWeaponIcon(value);
		}
	}
}