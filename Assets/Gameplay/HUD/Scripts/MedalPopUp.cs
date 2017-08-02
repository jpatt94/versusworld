using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MedalPopUp : MonoBehaviour
{
	[SerializeField]
	private float spacing;
	[SerializeField]
	private List<Sprite> sprites;
	[SerializeField]
	private GameObject entryPrefab;
	[SerializeField]
	private AudioClip soundEffect;

	private List<MedalPopUpEntry> entries;

	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		entries = new List<MedalPopUpEntry>();

		aud = GetComponentInParent<AudioSource>();
	}

	public void Update()
	{
		for (int i = 0; i < entries.Count; i++)
		{
			MedalPopUpEntry e = entries[i];
			if (e.CurrentTime > e.Duration || e.Alpha <= Mathf.Epsilon)
			{
				Destroy(e.gameObject);
				entries.RemoveAt(i);
				ArrangeMedals();
				break;
			}
		}
	}

	/**********************************************************/
	// Interface

	public void Trigger(MedalType type, bool playSound = true)
	{
		GameObject obj = Instantiate(entryPrefab);
		obj.transform.SetParent(transform, false);
		obj.GetComponent<Image>().sprite = sprites[(int)type];
		MedalPopUpEntry entry = obj.GetComponent<MedalPopUpEntry>();
		entry.Name = MedalTracker.GetMedalName(type);
		entries.Insert((entries.Count + 1) / 2, entry);

		ArrangeMedals();

		if (playSound)
		{
			aud.PlayOneShot(soundEffect);
		}
	}

	public void Trigger(int[] medals)
	{
		foreach (int m in medals)
		{
			Trigger((MedalType)m, false);
		}

		if (medals.Length > 0)
		{
			aud.PlayOneShot(soundEffect);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void ArrangeMedals()
	{
		float startX = ((entries.Count - 1) * spacing) * 0.5f;
		float currentX = startX;
		foreach (MedalPopUpEntry e in entries)
		{
			e.DesiredPosition = currentX;
			currentX -= spacing;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public List<Sprite> Sprites
	{
		get
		{
			return sprites;
		}
	}
}
