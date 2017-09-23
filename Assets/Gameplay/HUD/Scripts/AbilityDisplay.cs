using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDisplay : MonoBehaviour
{
	[SerializeField]
	private GameObject[] abilityIconPrefabs;
	[SerializeField]
	private float iconSeparation;

	private Dictionary<AbilityType, AbilityIcon> icons;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		icons = new Dictionary<AbilityType, AbilityIcon>();
	}

	/**********************************************************/
	// Interface

	public AbilityIcon AddAbilityIcon(AbilityType type)
	{
		if (icons.ContainsKey(type))
		{
			return icons[type];
		}

		GameObject obj = Instantiate(abilityIconPrefabs[(int)type], transform, false);

		AbilityIcon icon = obj.GetComponent<AbilityIcon>();
		icons.Add(type, icon);

		UpdateIconPositions();

		return icon;
	}

	public void RemoveAbilityIcon(AbilityType type)
	{
		if (icons.ContainsKey(type))
		{
			Destroy(icons[type].gameObject);
			icons.Remove(type);

			UpdateIconPositions();
		}
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateIconPositions()
	{
		int i = 0;
		foreach (var icon in icons)
		{
			icon.Value.transform.localPosition = Vector3.up * iconSeparation * i;
			i++;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			foreach (var icon in icons)
			{
				icon.Value.Visible = value;
			}
		}
	}

	public Dictionary<AbilityType, AbilityIcon> Icons
	{
		get
		{
			return icons;
		}
	}
}

public enum AbilityType
{
	Thrust,
	BigHead,
	DamageResist,
	SpeedBoost,
	DamageBoost,
	NumTypes,
}