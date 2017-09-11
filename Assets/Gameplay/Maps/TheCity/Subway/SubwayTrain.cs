using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SubwayTrain : SafeNetworkBehaviour
{
	[SyncVar]
	private float normalizedTime;

	private Vector3 lastRotation;

	private SubwayManager mgr;
	private Rigidbody rig;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	protected override bool Ready()
	{
		return FindObjectOfType<SubwayManager>();
	}

	protected override void DelayedAwake()
	{
		base.DelayedAwake();

		mgr = FindObjectOfType<SubwayManager>();
		rig = GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		if (initialized)
		{
			Vector3 prevPos = rig.position;
			Vector3 newPos = new Vector3(mgr.TrackXCurve.Evaluate(normalizedTime), rig.position.y, mgr.TrackZCurve.Evaluate(normalizedTime));
			rig.velocity = newPos - prevPos;
			rig.position += rig.velocity * Time.fixedDeltaTime;

			transform.forward = (newPos - prevPos).normalized;
			Vector3 ang = transform.rotation.eulerAngles - lastRotation;
			ang.y %= Mathf.PI * 2.0f;
			rig.angularVelocity = ang;

			lastRotation = transform.rotation.eulerAngles;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public float NormalizedTime
	{
		get
		{
			return normalizedTime;
		}
		set
		{
			normalizedTime = value;
		}
	}
}
