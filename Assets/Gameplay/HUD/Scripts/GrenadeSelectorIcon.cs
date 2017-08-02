using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GrenadeSelectorIcon : MonoBehaviour
{
	private int amount;

	private GrenadeSelector mgr;
	private RawImage iconImage;
	private Text amountText;

	/**********************************************************/
	// MonoBehaviour Interface

	void Awake()
	{
		amount = 0;

		iconImage = GetComponent<RawImage>();
		amountText = GetComponentInChildren<Text>();
	}

	/**********************************************************/
	// Accessors/Mutators

	public GrenadeSelector Manager
	{
		get
		{
			return mgr;
		}
		set
		{
			mgr = value;
		}
	}

	public bool Visible
	{
		get
		{
			return iconImage.enabled;
		}
		set
		{
			iconImage.enabled = value;
			amountText.enabled = value;
		}
	}

	public float Alpha
	{
		get
		{
			return iconImage.color.a;
		}
		set
		{
			Utility.SetAlpha(iconImage, value);
			Utility.SetAlpha(amountText, value);
		}
	}

	public int Amount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = value;
			amountText.text = amount.ToString();
		}
	}

	public Texture Texture
	{
		set
		{
			iconImage.texture = value;
		}
	}
}