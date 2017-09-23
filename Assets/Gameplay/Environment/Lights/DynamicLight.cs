using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DynamicLight : MonoBehaviour
{
	[SerializeField]
	private float maxIntensity;
	[SerializeField]
	private AnimationCurve intensityCurve;
	[SerializeField]
	private float speed;

	private Light lgt;
	private float time;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		lgt = GetComponent<Light>();
	}

	public void Update()
	{
		time += Time.deltaTime * speed;
		lgt.intensity = intensityCurve.Evaluate(time) * maxIntensity;
	}
}
