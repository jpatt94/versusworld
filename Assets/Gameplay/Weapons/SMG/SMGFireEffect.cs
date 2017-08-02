using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGFireEffect : MonoBehaviour
{
	[SerializeField]
	private float duration;
	[SerializeField]
	private Material[] materials;

	private float time;
	private int lastFrame;

	private LineRenderer line;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		line = GetComponent<LineRenderer>();
	}

	public void Update()
	{
		time -= Time.deltaTime;
		if (time < 0.0f)
		{
			line.enabled = false;
		}
	}

	/**********************************************************/
	// Interface

	public void Play()
	{
		time = duration;
		line.enabled = true;

		int frame = Random.Range(0, materials.Length);
		if (frame == lastFrame)
		{
			frame--;
			if (frame < 0)
			{
				frame = materials.Length - 1;
			}
		}

		line.material = materials[frame];

		lastFrame = frame;
	}
}
