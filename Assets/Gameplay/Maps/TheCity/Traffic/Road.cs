using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Road : MonoBehaviour
{
	[SerializeField]
	private int maxCars;
	[SerializeField]
	private float minSpawnTime;
	[SerializeField]
	private float maxSpawnTime;

	private LightStatus lightStatus;
	private float nextSpawnTime;
	private List<Vehicle> vehicles;

	private TrafficManager mgr;
	private Light[,] lights;
	private Transform spawnPoint;
	private float slowPosition;
	private float stopPosition;
	private float despawnPosition;
	private Vector3 direction;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		mgr = GetComponentInParent<TrafficManager>();

		Transform t = transform.Find("Lights");
		lights = new Light[3, 2];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				lights[i, j] = t.Find(((LightStatus)i).ToString() + "Light" + j).GetComponent<Light>();
				lights[i, j].gameObject.SetActive(false);
			}
		}

		spawnPoint = transform.Find("SpawnPoint");

		Transform despawnPoint = transform.Find("DespawnPoint");
		direction = despawnPoint.position - spawnPoint.position;
		despawnPosition = direction.magnitude;
		direction.Normalize();

		slowPosition = (transform.Find("SlowPoint").position - spawnPoint.position).magnitude;
		stopPosition = (transform.Find("StopPoint").position - spawnPoint.position).magnitude;

		vehicles = new List<Vehicle>();
	}

	public void Update()
	{
		if (mgr.isServer)
		{
			nextSpawnTime -= Time.deltaTime;
			if (nextSpawnTime <= 0.0f && vehicles.Count < maxCars)
			{
				SpawnVehicle();
				nextSpawnTime = Mathf.Lerp(minSpawnTime, maxSpawnTime, Random.value);
			}
		}
	}

	public void FixedUpdate()
	{
		if (mgr.isServer)
		{
			for (int i = 0; i < vehicles.Count; i++)
			{
				Vehicle veh = vehicles[i];
				float speed = veh.Speed;

				if (i > 0)
				{
					float dist = vehicles[i - 1].Position - veh.Position;
					speed *= mgr.SlowDownCurve.Evaluate(Mathf.InverseLerp(mgr.MinDistanceBetweenCars, mgr.MaxDistanceBetweenCars, dist));
				}

				if (lightStatus != LightStatus.Green && !veh.RunningRed)
				{
					speed *= mgr.SlowDownCurve.Evaluate(Mathf.InverseLerp(stopPosition, slowPosition, veh.Position));
				}

				veh.Position += speed * Time.fixedDeltaTime;
				if (veh.Position > despawnPosition)
				{
					NetworkServer.UnSpawn(veh.gameObject);
					Destroy(veh.gameObject);
					vehicles.RemoveAt(i);
					i--;
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public Vector3 RoadToGlobalPosition(float roadPosition)
	{
		return spawnPoint.position + direction * roadPosition;
	}

	/**********************************************************/
	// Helper Functions

	private void SpawnVehicle()
	{
		int matIndex;
		GameObject obj = Instantiate(mgr.GetRandomVehicle(out matIndex));
		obj.transform.position = spawnPoint.position;
		obj.transform.forward = direction;

		Vehicle veh = obj.GetComponent<Vehicle>();
		veh.Road = this;
		veh.MatIndex = matIndex;
		vehicles.Add(veh);

		NetworkServer.Spawn(obj);
	}

	private void OnChangeGreen()
	{
		for (int i = 0; i < vehicles.Count; i++)
		{
			vehicles[i].SpeedMult = 0.0f;
		}
	}

	private void OnChangeYellow()
	{
		for (int i = 0; i < vehicles.Count; i++)
		{
			Vehicle veh = vehicles[i];
			veh.RunningRed = veh.Position > slowPosition;
		}
	}

	private void OnChangeRed()
	{

	}

	/**********************************************************/
	// Accessors/Mutators

	public LightStatus LightStatus
	{
		get
		{
			return lightStatus;
		}
		set
		{
			if (lightStatus == value)
			{
				return;
			}

			for (int i = 0; i < 2; i++)
			{
				lights[(int)lightStatus, i].gameObject.SetActive(false);
			}

			lightStatus = value;

			for (int i = 0; i < 2; i++)
			{
				lights[(int)lightStatus, i].gameObject.SetActive(true);
			}

			switch (value)
			{
				case LightStatus.Green: OnChangeGreen(); break;
				case LightStatus.Yellow: OnChangeYellow(); break;
				case LightStatus.Red: OnChangeRed(); break;
			}
		}
	}

	public TrafficManager Manager
	{
		get
		{
			return mgr;
		}
	}
}

public enum LightStatus
{
	Green,
	Yellow,
	Red,
}
