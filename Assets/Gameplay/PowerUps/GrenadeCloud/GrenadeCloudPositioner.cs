using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeCloudPositioner : MonoBehaviour
{
	private RectTransform rect;
	private Button button;
	private Image mapOverviewImage;
	private Image coverageCircle;
	private MultiplayerMap map;
	private NetworkPowerUpCarrier carrier;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		rect = GetComponent<RectTransform>();
		button = GetComponent<Button>();
		mapOverviewImage = GetComponent<Image>();
		coverageCircle = transform.Find("CoverageCircle").GetComponent<Image>();
		map = GameObject.Find("MultiplayerMap").GetComponent<MultiplayerMap>();

		mapOverviewImage.sprite = map.MapOverview;

		PlayerInput.Enabled = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void Update()
	{
		Vector3 mouse = Input.mousePosition;
		Vector2 offset = mouse - transform.position;
		offset.x /= Utility.GetScaleFactorX();
		offset.y /= Utility.GetScaleFactorY();

		float offsetX = rect.offsetMax.x;
		float offsetY = rect.offsetMax.y;

		if (offset.x >= -offsetX && offset.y >= -offsetY && offset.x <= offsetX && offset.y <= offsetY)
		{
			coverageCircle.enabled = true;

			coverageCircle.transform.localPosition = offset;

			if (Input.GetMouseButtonDown(0))
			{
				PlayerManager.LocalPlayer.WeaponCarrier.NeedsShootingReset = true;
				PlayerInput.Enabled = true;
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;

				carrier.OnGrenadeCloudPositionerClick(map.ConvertMapUVToPosition(Mathf.InverseLerp(-offsetX, offsetX, offset.x), 1.0f - Mathf.InverseLerp(-offsetY, offsetY, offset.y)));
			}
		}
		else
		{
			coverageCircle.enabled = false;
		}
	}

	public void OnDestroy()
	{
		PlayerInput.Enabled = true;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	/**********************************************************/
	// Accessors/Mutators

	public NetworkPowerUpCarrier Carrier
	{
		set
		{
			carrier = value;
		}
	}

	public float CoverageDiameter
	{
		set
		{
			coverageCircle.rectTransform.sizeDelta = new Vector2(
				rect.sizeDelta.x * (value / map.Width),
				rect.sizeDelta.y * (value / map.Length));
		}
	}
}