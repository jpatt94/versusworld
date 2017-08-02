using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class KillFeed : MonoBehaviour
{
	[SerializeField]
	private float duration;
	[SerializeField]
	private int maxEntries;
	[SerializeField]
	private float gap;
	[SerializeField]
	private Color positiveColor;
	[SerializeField]
	private Color negativeColor;
	[SerializeField]
	private GameObject entryPrefab;
	[SerializeField]
	private GameObject[] killTypeImages;

	private Queue<KillFeedEntry> entries;

	private HUD mgr;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		entries = new Queue<KillFeedEntry>();

		mgr = GetComponentInParent<HUD>();
	}

	public void Update()
	{
		if (entries.Count > 0 && entries.Peek().TimeLeft <= 0.0f)
		{
			Destroy(entries.Peek().gameObject);
			entries.Dequeue();
		}

		foreach (KillFeedEntry e in entries)
		{
			if (e.RectTransform.localPosition.y > (maxEntries - 1) * gap)
			{
				e.AlphaOverride = Mathf.Clamp01(1.0f - (e.RectTransform.localPosition.y - ((maxEntries - 1) * gap)) / gap);
			}
			else
			{
				break;
			}
		}
	}

	/**********************************************************/
	// Interface

	public void ShowMessage(string str, int colorPositivity)
	{
		ShowMessage(str, colorPositivity == 0 ? Color.white : (colorPositivity == 1 ? positiveColor : negativeColor));
	}

	public void ShowMessage(string str, Color color)
	{
		foreach (KillFeedEntry e in entries)
		{
			e.DesiredY += gap;
		}

		GameObject obj = Instantiate(entryPrefab);
		obj.GetComponent<RectTransform>().localPosition = Vector3.zero;
		obj.transform.SetParent(transform, false);

		KillFeedEntry entry = obj.GetComponent<KillFeedEntry>();
		entry.Manager = this;
		entry.DesiredY = 0.0f;
		entry.TimeLeft = duration;
		entry.SetMessage(str, color);
		entries.Enqueue(entry);
	}

	public void ShowKill(string killerName, Color killerColor, string victimName, Color victimColor, DamageType killType, int[] medals)
	{
		foreach (KillFeedEntry e in entries)
		{
			e.DesiredY += gap;
		}

		GameObject obj = Instantiate(entryPrefab);
		obj.GetComponent<RectTransform>().localPosition = Vector3.zero;
		obj.transform.SetParent(transform, false);

		KillFeedEntry entry = obj.GetComponent<KillFeedEntry>();
		entry.Manager = this;
		entry.DesiredY = 0.0f;
		entry.TimeLeft = duration;
		entry.ShowKill(killerName, killerColor, victimName, victimColor, killType, medals);
		entries.Enqueue(entry);
	}

	public void OnKill(KillData data, int[] medals)
	{
		if (data.shooter == data.victim)
		{
			Color c = Color.white;
			if (PlayerManager.LocalPlayerID == data.shooter)
			{
				c = negativeColor;
			}

			ShowMessage(PlayerManager.PlayerMap[data.shooter].Name + " committed suicide", c);
		}
		else if (data.shooter < 0)
		{
			Color c = Color.white;
			if (PlayerManager.LocalPlayerID == data.victim)
			{
				c = negativeColor;
			}

			ShowMessage(PlayerManager.PlayerMap[data.victim].Name + " was killed by nature", c);
		}
		else if (PartyManager.SameTeam(data.shooter, data.victim))
		{
			ShowMessage(PlayerManager.PlayerMap[data.shooter].Name + " betrayed " + PlayerManager.PlayerMap[data.victim].Name, Color.white);
		}
		else
		{
			ShowKill(PlayerManager.PlayerMap[data.shooter].Name, 
				data.shooter == PlayerManager.LocalPlayerID ? positiveColor : Color.white, 
				PlayerManager.PlayerMap[data.victim].Name,
				data.victim == PlayerManager.LocalPlayerID ? positiveColor : Color.white, 
				data.damageType, medals);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public GameObject GetKillTypeImage(int killType)
	{
		return killTypeImages[killType];
	}

	public Sprite GetMedalImage(MedalType medalType)
	{
		return mgr.MedalPopUp.Sprites[(int)medalType];
	}
}
