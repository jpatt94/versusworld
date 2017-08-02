using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{
	[SerializeField]
	private GameObject entryPrefab;
	[SerializeField]
	private AudioClip hitSound;
	[SerializeField]
	private AudioClip headShotHitSound;
	[SerializeField]
	private AudioClip friendlyHitSound;
	[SerializeField]
	private float explosiveHitMarkerDuration;
	[SerializeField]
	private float friendlyHitMarkerDuration;

	private float explosiveHitMarkerTime;
	private float friendlyHitMarkerTime;

	private Image explosiveHitMarker;
	private Image friendlyHitMarker;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		aud = GetComponentInParent<AudioSource>();
		explosiveHitMarker = aud.transform.Find("ShakeCanvas/ExplosiveHitMarker").GetComponent<Image>();
		friendlyHitMarker = transform.Find("Friendly").GetComponent<Image>();
	}

	public void Update()
	{
		explosiveHitMarkerTime -= Time.deltaTime;
		Utility.SetAlpha(explosiveHitMarker, explosiveHitMarkerTime / explosiveHitMarkerDuration);

		friendlyHitMarkerTime -= Time.deltaTime;
		Utility.SetAlpha(friendlyHitMarker, friendlyHitMarkerTime / friendlyHitMarkerDuration);
	}

	/**********************************************************/
	// Interface

	public void Trigger(HitMarkerType type)
	{
		if (type == HitMarkerType.Explosive)
		{
			explosiveHitMarkerTime = explosiveHitMarkerDuration;
		}
		else if (type == HitMarkerType.SuppressSniper)
		{
			GameObject obj = Instantiate(entryPrefab);
			obj.transform.SetParent(transform, false);
			obj.GetComponent<HitMarkerEntry>().SetSuppressSniper();
		}
		else if (type == HitMarkerType.Friendly)
		{
			friendlyHitMarkerTime = friendlyHitMarkerDuration;
			aud.PlayOneShot(friendlyHitSound);
		}
		else
		{
			GameObject obj = Instantiate(entryPrefab);
			obj.transform.SetParent(transform, false);
			obj.GetComponent<HitMarkerEntry>().SetHeadShot(type == HitMarkerType.HeadShot);
		}

		if (type != HitMarkerType.Friendly)
		{
			aud.PlayOneShot(type == HitMarkerType.HeadShot ? headShotHitSound : hitSound);
		}
	}
}

public enum HitMarkerType
{
	Default,
	HeadShot,
	Explosive,
	SuppressSniper,
	Friendly,
}