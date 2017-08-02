using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Waypoint : MonoBehaviour
{
	private Text distanceText;
	private Transform arrowTransform;
	private Camera cam;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		Transform t = transform.Find("DistanceText");
		if (t)
		{
			distanceText = t.GetComponent<Text>();
		}
		arrowTransform = transform.Find("Arrow");

		transform.SetParent(HUD.Instance.StaticCanvas.transform.Find("WeaponWaypoints"), false);
	}

	/**********************************************************/
	// Accessors/Mutators

	public Camera Camera
	{
		set
		{
			cam = value;
		}
	}

	public Vector3 WorldPosition
	{
		set
		{
			Vector3 screenPos = cam.WorldToScreenPoint(value);
			if (screenPos.z > 0.0f && screenPos.x > Screen.width * HUD.WaypointMin.x && screenPos.y > Screen.height * HUD.WaypointMin.y &&
				screenPos.x < Screen.width * HUD.WaypointMax.x && screenPos.y < Screen.height * HUD.WaypointMax.y)
			{
				transform.position = screenPos;
				if (arrowTransform)
				{
					arrowTransform.transform.localRotation = Quaternion.identity;
				}
			}
			else
			{
				if (screenPos.z < 0.0f)
				{
					screenPos.x = Screen.width - screenPos.x;
					screenPos.y = Screen.height - screenPos.y;
				}

				Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0.0f) * 0.5f;
				screenPos -= screenCenter;

				float angle = Mathf.Atan2(screenPos.y, screenPos.x);
				angle -= 90.0f * Mathf.Deg2Rad;

				float cos = Mathf.Cos(angle);
				float sin = -Mathf.Sin(angle);
				float m = cos / sin;

				Vector3 screenBounds = screenCenter;
				screenBounds.y *= 1.0f - ((cos > 0.0f ? (1.0f - HUD.WaypointMax.y) : HUD.WaypointMin.y) * 2.0f);
				screenPos = new Vector3(screenBounds.y / m, screenBounds.y, 0.0f) * (cos > 0.0f ? 1.0f : -1.0f);

				screenBounds.x *= 1.0f - ((screenPos.x > 0 ? (1.0f - HUD.WaypointMax.x) : HUD.WaypointMin.x) * 2.0f);
				if (screenPos.x > screenBounds.x)
				{
					screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0.0f);
				}
				else if (screenPos.x < -screenBounds.x)
				{
					screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0.0f);
				}

				screenPos += screenCenter;

				transform.position = screenPos;
				if (arrowTransform)
				{
					arrowTransform.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle * Mathf.Rad2Deg + 180.0f);
				}
			}

			if (distanceText)
			{
				distanceText.text = Mathf.CeilToInt((value - cam.transform.position).magnitude).ToString() + "m";
			}
		}
	}
}