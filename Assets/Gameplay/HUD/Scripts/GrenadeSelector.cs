using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GrenadeSelector : MonoBehaviour
{
	[SerializeField]
	private float separationY;
	[SerializeField]
	private AnimationCurve scaleCurve;
	[SerializeField]
	private Texture[] iconTextures;
	[SerializeField]
	private GameObject iconPrefab;

	private Image arrowImage;
	private SortedDictionary<GrenadeType, GrenadeSelectorIcon> icons;
	private GrenadeType selectedType;
	private bool visible;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		arrowImage = transform.Find("Arrow").GetComponent<Image>();
		icons = new SortedDictionary<GrenadeType, GrenadeSelectorIcon>();
		visible = true;
	}

	public void Update()
	{
		if (visible)
		{
			if (icons.ContainsKey(selectedType))
			{
				arrowImage.enabled = true;
				Vector3 pos = arrowImage.transform.localPosition;
				int i = 0;
				foreach (KeyValuePair<GrenadeType, GrenadeSelectorIcon> kv in icons)
				{
					if (kv.Key == selectedType)
					{
						pos.y = -separationY * i;
						break;
					}
					i++;
				}
				arrowImage.transform.localPosition = pos;
			}
			else
			{
				arrowImage.enabled = false;
			}
		}
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateIconPositions()
	{
		int i = 0;
		foreach (KeyValuePair<GrenadeType, GrenadeSelectorIcon> kv in icons)
		{
			kv.Value.transform.localPosition = Vector3.down * separationY * i;
			i++;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			visible = value;
			foreach (KeyValuePair<GrenadeType, GrenadeSelectorIcon> kv in icons)
			{
				kv.Value.Visible = value;
			}
			arrowImage.enabled = value;
		}
	}

	public int GetGrenadeAmount(GrenadeType type)
	{
		if (icons.ContainsKey(type))
		{
			return icons[type].Amount;
		}

		return 0;
	}

	public void SetGrenadeAmount(GrenadeType type, int value)
	{
		value = Mathf.Max(0, value);

		if (!icons.ContainsKey(type))
		{
			if (value > 0)
			{
				GameObject obj = Instantiate(iconPrefab, Vector3.zero, Quaternion.identity);
				obj.transform.SetParent(transform, false);
				icons[type] = obj.GetComponent<GrenadeSelectorIcon>();
				icons[type].Manager = this;
				icons[type].Texture = iconTextures[(int)type];
				icons[type].Visible = visible;

				icons[type].Amount = value;
			}
		}
		else
		{
			icons[type].Amount = value;

			if (value == 0)
			{
				Destroy(icons[type].gameObject);
				icons.Remove(type);
			}
		}

		UpdateIconPositions();
	}

	public GrenadeType SelectedType
	{
		set
		{
			selectedType = value;
		}
	}
}
