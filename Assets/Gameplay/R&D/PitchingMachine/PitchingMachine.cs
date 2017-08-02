using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PitchingMachine : NetworkBehaviour
{
	[SerializeField]
	private float pitchRate;
	[SerializeField]
	private float speed;
	[SerializeField]
	private GrenadeType grenadeType;

	private float canPitch;

	private GrenadeManager mgr;
	private Transform cannon;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public void Awake()
	{
		cannon = transform.Find("Cannon");
	}

	public void Update()
	{
		if (isServer)
		{
			canPitch += Time.deltaTime;
			if (canPitch >= 0.0f)
			{
				if (GrenadeManager)
				{
					GrenadeManager.CreateLiveGrenade(grenadeType, cannon.position, cannon.up, speed);
				}

				canPitch = -(1.0f / pitchRate);
			}
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public GrenadeManager GrenadeManager
	{
		get
		{
			if (!mgr)
			{
				GameObject obj = GameObject.Find("GrenadeManager");
				if (obj)
				{
					mgr = obj.GetComponent<GrenadeManager>();
				}
			}
			return mgr;
		}
	}
}