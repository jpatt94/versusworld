using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Vehicle : SafeNetworkBehaviour
{
	[SerializeField]
	private float minSpeed;
	[SerializeField]
	private float maxSpeed;
	[SerializeField]
	private float tireCircumference;
	[SerializeField]
	private float accelerationRate;

	[SyncVar]
	private float position;

	private float speed;
	private bool runningRed;
	private float speedMult;
	private int matIndex;

	private Road road;
	private Rigidbody rig;
	private Transform[] wheels;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	protected override bool Ready()
	{
		return road != null;
	}

	protected override void DelayedAwake()
	{
		base.DelayedAwake();

		speed = Mathf.Lerp(minSpeed, maxSpeed, Random.value);

		rig = GetComponent<Rigidbody>();
		wheels = new Transform[4];
		for (int i = 0; i < 4; i++)
		{
			wheels[i] = transform.Find("Wheel" + i);
		}

		speedMult = 1.0f;
	}

	protected override void DelayedOnStartServer()
	{
		base.DelayedOnStartServer();

		RpcSetAttrs(road.gameObject.name, matIndex);
	}

	public override void Update()
	{
		base.Update();

		if (road != null)
		{
			speedMult = Mathf.Clamp01(speedMult + accelerationRate * Time.deltaTime);

			Vector3 prevPos = rig.position;
			Vector3 newPos = road.RoadToGlobalPosition(position);
			rig.velocity = newPos - prevPos;
			rig.position += rig.velocity * Time.fixedDeltaTime;

			foreach (Transform t in wheels)
			{
				t.localRotation = Quaternion.Euler(Vector3.right * (position / tireCircumference) * 360.0f);
			}
		}
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcSetAttrs(string roadName, int matIndex)
	{
		road = GameObject.Find("TrafficManager/" + roadName).GetComponent<Road>();

		foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
		{
			m.material = road.Manager.VehicleMaterials[matIndex];
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public Road Road
	{
		get
		{
			return road;
		}
		set
		{
			road = value;
		}
	}

	public float Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public float Speed
	{
		get
		{
			return speed * speedMult;
		}
		set
		{
			speed = value;
		}
	}

	public bool RunningRed
	{
		get
		{
			return runningRed;
		}
		set
		{
			runningRed = value;
		}
	}

	public float SpeedMult
	{
		get
		{
			return speedMult;
		}
		set
		{
			speedMult = value;
		}
	}

	public int MatIndex
	{
		set
		{
			matIndex = value;
		}
	}
}
