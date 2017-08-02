using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageIndicator : MonoBehaviour
{
	public GameObject entryPrefab;

	private Dictionary<int, DamageIndicatorEntry> entries;

	void Start()
	{
		entries = new Dictionary<int, DamageIndicatorEntry>();
	}

	/**********************************************************/
	// Interface

	public void Trigger(int shooterID, Vector3 position, CameraManager cam)
	{
		DamageIndicatorEntry entry = null;

		foreach (KeyValuePair<int, DamageIndicatorEntry> kv in entries)
		{
			if (kv.Value.ShooterID == shooterID)
			{
				entry = kv.Value;
				break;
			}
		}

		if (!entry)
		{
			GameObject obj = Instantiate(entryPrefab);
			obj.transform.SetParent(transform, false);
			entry = obj.GetComponent<DamageIndicatorEntry>();
			entry.ShooterID = shooterID;
			entries[shooterID] = entry;
		}

		entry.Initialize(position, cam, this);
	}

	public void Clear()
	{
		foreach (KeyValuePair<int, DamageIndicatorEntry> kv in entries)
		{
			Destroy(kv.Value.gameObject);
		}
		entries.Clear();
	}

	public void OnFinish(int shooterID)
	{
		Destroy(entries[shooterID].gameObject);
		entries.Remove(shooterID);
	}
}
