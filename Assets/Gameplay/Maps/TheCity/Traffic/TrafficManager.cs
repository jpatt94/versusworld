using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TrafficManager : NetworkBehaviour
{
	[SerializeField]
	private float halfCycleDuration;
	[SerializeField]
	private float yellowLightDuration;
	[SerializeField]
	private GameObject[] vehiclePrefabs;
	[SerializeField]
	private AnimationCurve slowDownCurve;
	[SerializeField]
	private float minDistanceBetweenCars;
	[SerializeField]
	private float maxDistanceBetweenCars;
	[SerializeField]
	private Material[] vehicleMaterials;
	[SerializeField]
	private bool[] useAllMaterials;

	private float cycleTime;

	private Road northRoad;
	private Road southRoad;
	private Road westRoad;
	private Road eastRoad;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public void Awake()
	{
		northRoad = transform.Find("NorthRoad").GetComponent<Road>();
		southRoad = transform.Find("SouthRoad").GetComponent<Road>();
		westRoad = transform.Find("WestRoad").GetComponent<Road>();
		eastRoad = transform.Find("EastRoad").GetComponent<Road>();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		ResetCycle();
	}

	public void Update()
	{
		cycleTime -= Time.deltaTime;

		if (cycleTime < yellowLightDuration)
		{
			westRoad.LightStatus = LightStatus.Red;
			eastRoad.LightStatus = LightStatus.Red;
		}
		else if (cycleTime < yellowLightDuration * 2)
		{
			westRoad.LightStatus = LightStatus.Yellow;
			eastRoad.LightStatus = LightStatus.Yellow;
		}
		else if (cycleTime < halfCycleDuration)
		{
			westRoad.LightStatus = LightStatus.Green;
			eastRoad.LightStatus = LightStatus.Green;
		}
		else if (cycleTime < halfCycleDuration + yellowLightDuration)
		{
			northRoad.LightStatus = LightStatus.Red;
			southRoad.LightStatus = LightStatus.Red;
		}
		else if (cycleTime < halfCycleDuration + yellowLightDuration * 2)
		{
			northRoad.LightStatus = LightStatus.Yellow;
			southRoad.LightStatus = LightStatus.Yellow;
		}

		if (isServer)
		{
			if (cycleTime <= 0.0f)
			{
				ResetCycle();
			}
		}
	}

	/**********************************************************/
	// Interface

	public GameObject GetRandomVehicle(out int matIndex)
	{
		int i = Random.Range(0, vehiclePrefabs.Length);
		matIndex = useAllMaterials[i] ? Random.Range(0, vehicleMaterials.Length) : 0;
		return vehiclePrefabs[i];
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcResetCycle()
	{
		if (!isServer)
		{
			ResetCycle();
		}
	}

	/**********************************************************/
	// Helper Functions

	private void ResetCycle()
	{
		cycleTime = halfCycleDuration * 2;
		northRoad.LightStatus = LightStatus.Green;
		southRoad.LightStatus = LightStatus.Green;
		westRoad.LightStatus = LightStatus.Red;
		eastRoad.LightStatus = LightStatus.Red;

		if (isServer)
		{
			RpcResetCycle();
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public AnimationCurve SlowDownCurve
	{
		get
		{
			return slowDownCurve;
		}
	}

	public float MinDistanceBetweenCars
	{
		get
		{
			return minDistanceBetweenCars;
		}
	}

	public float MaxDistanceBetweenCars
	{
		get
		{
			return maxDistanceBetweenCars;
		}
	}

	public Material[] VehicleMaterials
	{
		get
		{
			return vehicleMaterials;
		}
	}
}
