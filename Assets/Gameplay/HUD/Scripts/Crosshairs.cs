using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Crosshairs : HUDElement
{
	[SerializeField]
	private float[] popScale;
	[SerializeField]
	private float[] popDuration;
	[SerializeField]
	private Color enemyColor;

	private int activeCrosshair;
	private Color color;
	private float popTime;

	private Image[] crosshairs;

	/**********************************************************/
	// Interface

	public void Start()
	{
		activeCrosshair = -1;
		color = Color.white;

		crosshairs = new Image[(int)WeaponType.None];
		crosshairs[(int)WeaponType.AssaultRifle] = Utility.FindChild(gameObject, "AssaultRifleCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.SMG] = Utility.FindChild(gameObject, "SMGCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.Sniper] = Utility.FindChild(gameObject, "SniperCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.RocketLauncher] = Utility.FindChild(gameObject, "RocketLauncherCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.Shotgun] = Utility.FindChild(gameObject, "ShotgunCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.Pistol] = Utility.FindChild(gameObject, "PistolCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.MachineGun] = Utility.FindChild(gameObject, "MachineGunCrosshair").GetComponent<Image>();
		crosshairs[(int)WeaponType.Blaster] = Utility.FindChild(gameObject, "BlasterCrosshair").GetComponent<Image>();
	}

	public void Update()
	{
		popTime -= Time.deltaTime;
		if (activeCrosshair > -1)
		{
			transform.localScale = Vector3.one * Mathf.Lerp(1.0f, popScale[activeCrosshair], popTime / popDuration[activeCrosshair]);
		}
	}

	/**********************************************************/
	// Interface

	public void SetType(WeaponType type)
	{
		activeCrosshair = (int)type;
		crosshairs[activeCrosshair].color = color;

		for (int i = 0; i < (int)WeaponType.None; i++)
		{
			crosshairs[i].enabled = activeCrosshair == i;
		}

		popTime = 0.0f;
	}

	public void Pop()
	{
		popTime = popDuration[activeCrosshair];
	}

	public bool EnemyInCrosshairs
	{
		set
		{
			if (activeCrosshair > -1)
			{
				Color color = value ? enemyColor : Color.white;
				color.a = crosshairs[activeCrosshair].color.a;
				crosshairs[activeCrosshair].color = color;
			}
		}
	}

	public float Alpha
	{
		get
		{
			return color.a;
		}
		set
		{
			color.a = value;
			if (activeCrosshair > -1)
			{
				crosshairs[activeCrosshair].color = color;
			}
		}
	}
}
