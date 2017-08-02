using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class WeaponDrop : NetworkBehaviour
{
	[SerializeField]
	private WeaponType type;
	[SerializeField]
	private bool despawns;
	[SerializeField]
	private float despawnTime;
	[SerializeField]
	private GameObject waypointPrefab;

	[SyncVar]
	private int id;
	[SyncVar]
	private bool showWaypoint;

	private bool hasWaypoint;

	private WeaponData data;
	private WeaponManager mgr;
	private WeaponSpawner spawner;
	private WeaponWaypoint waypoint;

	/**********************************************************/
	// MonoBehavour Interface

	public void Awake()
	{
		GetComponent<Rigidbody>().maxDepenetrationVelocity = 3.0f;
	}

	public void Update()
	{
		if (isServer)
		{
			if (despawns)
			{
				despawnTime -= Time.deltaTime;
				if (despawnTime <= 0.0f)
				{
					Manager.DespawnWeapon(this);
				}
			}
			showWaypoint = spawner && hasWaypoint;
		}
	}

	public void LateUpdate()
	{
		if (showWaypoint)
		{
			if (!waypoint && PlayerManager.LocalPlayer)
			{
				waypoint = Instantiate(waypointPrefab).GetComponent<WeaponWaypoint>();
				waypoint.Camera = PlayerManager.LocalPlayer.Cam.WorldCamera;
				waypoint.WeaponType = type;
			}
			if (waypoint)
			{
				waypoint.WorldPosition = transform.position + Vector3.up;
			}
		}
	}

	public void OnDestroy()
	{
		if (waypoint)
		{
			Destroy(waypoint.gameObject);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public WeaponData Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public WeaponManager Manager
	{
		get
		{
			return mgr;
		}
		set
		{
			mgr = value;
		}
	}

	public WeaponSpawner Spawner
	{
		get
		{
			return spawner;
		}
		set
		{
			spawner = value;
		}
	}

	public WeaponType WeaponType
	{
		get
		{
			return type;
		}
	}

	public bool Despawns
	{
		get
		{
			return despawns;
		}
		set
		{
			despawns = value;
		}
	}

	public bool HasWaypoint
	{
		get
		{
			return hasWaypoint;
		}
		set
		{
			hasWaypoint = value;
		}
	}
}
