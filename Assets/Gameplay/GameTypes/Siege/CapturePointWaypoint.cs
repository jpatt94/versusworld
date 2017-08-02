using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapturePointWaypoint : Waypoint
{
	private Image backgroundImage;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		backgroundImage = transform.Find("Background").GetComponent<Image>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public float CaptureAmount
	{
		set
		{
			if (value > 0.5f)
			{
				Utility.SetRGB(backgroundImage, Color.Lerp(Color.white, PartyManager.GetTeamColor(0), (value - 0.5f) / 0.5f));
			}
			else
			{
				Utility.SetRGB(backgroundImage, Color.Lerp(PartyManager.GetTeamColor(1), Color.white, value / 0.5f));
			}
		}
	}
}