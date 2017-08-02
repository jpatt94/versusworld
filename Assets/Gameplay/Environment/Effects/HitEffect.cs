using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
	[SerializeField]
	private SurfaceType type;
	[SerializeField]
	private float duration;

	private float timeLeft;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		GetComponent<AudioSource>().PlayOneShot(EnvironmentManager.GetRandomHitEffectSound(type));
		timeLeft = duration;
	}

	public void Update()
	{
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f)
		{
			Destroy(gameObject);
		}
	}
}