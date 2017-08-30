using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
	[SerializeField]
	private Sprite[] weaponIcons;
	[SerializeField]
	private float iconPopDuration;
	[SerializeField]
	private float iconPopScale;
	[SerializeField]
	private AnimationCurve iconPopCurve;
	[SerializeField]
	private Color gainedAmmoColor;
	[SerializeField]
	private Color noAmmoColor;
	[SerializeField]
	private Color warningColor;

	private int ammoInClip;
	private int ammoInReserves;
	private InfiniteAmmoType infiniteAmmoType;
	private WeaponType secondaryWeaponType;

	private Image background;
	private Text clipText;
	private Text reservesText;
	private Image primaryWeaponIcon;
	private Image secondaryWeaponIcon;
	private Text slashText;
	private Image clipInfiniteSymbol;
	private Image reservesInfiniteSymbol;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		background = GetComponent<Image>();
		clipText = transform.Find("AmmoClip").GetComponent<Text>();
		reservesText = transform.Find("AmmoReserves").GetComponent<Text>();
		primaryWeaponIcon = transform.Find("PrimaryWeaponIcon").GetComponent<Image>();
		secondaryWeaponIcon = transform.Find("SecondaryWeaponIcon").GetComponent<Image>();
		slashText = transform.Find("SlashText").GetComponent<Text>();
		clipInfiniteSymbol = clipText.transform.Find("InfiniteSymbol").GetComponent<Image>();
		reservesInfiniteSymbol = reservesText.transform.Find("InfiniteSymbol").GetComponent<Image>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		get
		{
			return slashText.enabled;
		}
		set
		{
			background.enabled = value;
			clipText.enabled = value && infiniteAmmoType < InfiniteAmmoType.BottomlessClip;
			reservesText.enabled = value && infiniteAmmoType < InfiniteAmmoType.InfiniteAmmo;
			primaryWeaponIcon.enabled = value;
			secondaryWeaponIcon.enabled = value && secondaryWeaponType != WeaponType.None;
			slashText.enabled = value;
			clipInfiniteSymbol.enabled = value && infiniteAmmoType >= InfiniteAmmoType.BottomlessClip;
			reservesInfiniteSymbol.enabled = value && infiniteAmmoType >= InfiniteAmmoType.InfiniteAmmo;
		}
	}

	public int AmmoInClip
	{
		set
		{
			ammoInClip = value;
			clipText.text = value.ToString();

			if (infiniteAmmoType != InfiniteAmmoType.BottomlessClip)
			{
				if (value == 0)
				{
					clipText.color = ammoInReserves == 0 ? noAmmoColor : warningColor;
				}
				else
				{
					clipText.color = Color.white;
				}

				if (ammoInClip == 0 && ammoInReserves == 0)
				{
					slashText.color = noAmmoColor;
				}
				else
				{
					slashText.color = Color.white;
				}
			}
		}
	}

	public int AmmoInReserves
	{
		set
		{
			ammoInReserves = value;
			reservesText.text = value.ToString();

			if (infiniteAmmoType == InfiniteAmmoType.None)
			{
				if (value == 0)
				{
					reservesText.color = noAmmoColor;
				}
				else
				{
					reservesText.color = Color.white;
				}

				if (ammoInClip == 0 && ammoInReserves == 0)
				{
					slashText.color = noAmmoColor;
				}
				else
				{
					slashText.color = Color.white;
				}
			}
		}
	}

	public WeaponType PrimaryWeapon
	{
		set
		{
			primaryWeaponIcon.sprite = weaponIcons[(int)value];
		}
	}

	public WeaponType SecondaryWeapon
	{
		set
		{
			secondaryWeaponType = value;

			if (value != WeaponType.None)
			{
				secondaryWeaponIcon.enabled = true;
				secondaryWeaponIcon.sprite = weaponIcons[(int)value];
			}
			else
			{
				secondaryWeaponIcon.enabled = false;
			}
		}
	}

	public Sprite[] WeaponIcons
	{
		get
		{
			return weaponIcons;
		}
	}

	public InfiniteAmmoType InfiniteAmmoType
	{
		get
		{
			return infiniteAmmoType;
		}
		set
		{
			infiniteAmmoType = value;

			clipText.enabled = true;
			reservesText.enabled = true;
			clipInfiniteSymbol.enabled = false;
			reservesInfiniteSymbol.enabled = false;

			if (value >= InfiniteAmmoType.InfiniteAmmo)
			{
				reservesText.enabled = false;
				reservesInfiniteSymbol.enabled = true;
			}
			if (value >= InfiniteAmmoType.BottomlessClip)
			{
				clipText.enabled = false;
				clipInfiniteSymbol.enabled = true;
			}
		}
	}
}