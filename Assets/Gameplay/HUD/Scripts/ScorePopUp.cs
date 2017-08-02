using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScorePopUp : MonoBehaviour
{
	[SerializeField]
	private GameObject entryPrefab;
	[SerializeField]
	private AudioClip triggerSound;
	[SerializeField]
	private Color positiveColor;
	[SerializeField]
	private Color negativeColor;

	private Queue<ScorePopUpEntry> entries;

	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		entries = new Queue<ScorePopUpEntry>();

		aud = GetComponentInParent<AudioSource>();
	}

	public void Update()
	{
		if (entries.Count > 0 && entries.Peek().CurrentTime > entries.Peek().duration)
		{
			Destroy(entries.Peek().gameObject);
			entries.Dequeue();
		}
	}

	/**********************************************************/
	// Interface

	public void OnKill(KillData data)
	{
		if (data.shooter == PlayerManager.LocalPlayerID)
		{
			if (data.shooter == data.victim)
			{
				Trigger("Suicide", negativeColor);
			}
			else if (PartyManager.SameTeam(data.shooter, data.victim))
			{
				Trigger("Betrayal", negativeColor);
			}
			else
			{
				Trigger("+100 Kill", positiveColor);
			}
		}
	}

	public void Trigger(string str, Color color)
	{
		foreach (ScorePopUpEntry e in entries)
		{
			e.ChainPosition += 1.0f;
		}

		GameObject obj = Instantiate(entryPrefab);
		obj.transform.SetParent(transform, false);
		obj.transform.localPosition = Vector3.zero;
		ScorePopUpEntry entry = obj.GetComponent<ScorePopUpEntry>();
		entry.Text.text = str;
		entry.Text.color = color;
		entries.Enqueue(entry);

		aud.PlayOneShot(triggerSound);
	}
}
