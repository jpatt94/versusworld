using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
	[SerializeField]
	private float minTimeBetweenFlicker;
	[SerializeField]
	private float maxTimeBetweenFlicker;
	[SerializeField]
	private float minFlickerDuration;
	[SerializeField]
	private float maxFlickerDuration;
	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float minFlickerIntensityPercent;
	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float maxFlickerIntensityPercent;
	[SerializeField]
	private float changeSpeed;

	private float nextFlicker;
	private float flickerTime;
	private float maxIntensity;
	private float flickerIntensity;

	private Light lgt;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		lgt = GetComponent<Light>();
		aud = GetComponent<AudioSource>();

		maxIntensity = lgt.intensity;

		nextFlicker = Random.Range(minTimeBetweenFlicker, maxTimeBetweenFlicker);
	}

	public void Update()
	{
		flickerTime -= Time.deltaTime;
		lgt.intensity = Mathf.Max(flickerIntensity, Mathf.Min(maxIntensity, lgt.intensity + ((flickerTime > 0.0f) ? -changeSpeed : changeSpeed) * Time.deltaTime));

		nextFlicker -= Time.deltaTime;
		if (nextFlicker <= 0.0f)
		{
			nextFlicker = Random.Range(minTimeBetweenFlicker, maxTimeBetweenFlicker);
			flickerTime = Random.Range(minFlickerDuration, maxFlickerDuration);
			flickerIntensity = maxIntensity * Random.Range(minFlickerIntensityPercent, maxFlickerIntensityPercent);

			if (aud)
			{
				aud.PlayOneShot(EnvironmentManager.GetRandomFlickerSound());
			}
		}
	}
}
