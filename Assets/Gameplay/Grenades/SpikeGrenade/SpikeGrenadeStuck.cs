using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeGrenadeStuck : MonoBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab;
	[SerializeField]
	private AudioClip beepSound;
	[SerializeField]
	private AnimationCurve beepCurve;
	[SerializeField]
	private float beepLightIntensity;
	[SerializeField]
	private float beepLightDuration;

	private float fuseTime;
	private float nextBeep;
	private float beepLightTime;

	private AudioSource aud;
	private Light beepLight;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		aud = GetComponent<AudioSource>();
		beepLight = GetComponent<Light>();
	}

	public void Update()
	{
		fuseTime -= Time.deltaTime;
		if (fuseTime <= nextBeep)
		{
			aud.PlayOneShot(beepSound);
			beepLightTime = beepLightDuration;
			nextBeep -= beepCurve.Evaluate(nextBeep);
		}

		beepLight.intensity = Mathf.Clamp01(beepLightTime / beepLightDuration) * beepLightIntensity;
		beepLightTime -= Time.deltaTime;
	}

	public void OnDestroy()
	{
		GameObject obj = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(obj, 3.0f);
	}

	/**********************************************************/
	// Accessors/Mutators

	public float FuseTime
	{
		get
		{
			return fuseTime;
		}
		set
		{
			fuseTime = value;
			nextBeep = value;
		}
	}
}
