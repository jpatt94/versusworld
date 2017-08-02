using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PowerUp : NetworkBehaviour
{
	[SerializeField]
	private GameObject waypointPrefab;

	[SyncVar]
	private int id;
	[SyncVar]
	private bool available;

	private float respawnTime;
	private bool respawns;

	private MeshRenderer mesh;
	private Waypoint waypoint;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		id = -1;
		available = false;

		mesh = GetComponentInChildren<MeshRenderer>();
	}

	public void Update()
	{
		mesh.enabled = available;

		if (isServer && respawns && !available)
		{
			respawnTime -= Time.deltaTime;
			if (respawnTime <= 0.0f)
			{
				available = true;
			}
		}
	}

	public void LateUpdate()
	{
		if (available)
		{
			//if (!waypoint && PlayerManager.LocalPlayer)
			//{
			//	waypoint = Instantiate(waypointPrefab).GetComponent<Waypoint>();
			//	waypoint.Camera = PlayerManager.LocalPlayer.Cam.WorldCamera;
			//}
			//if (waypoint)
			//{
			//	waypoint.WorldPosition = transform.position + Vector3.up;
			//}
		}
		else if (waypoint)
		{
			Destroy(waypoint.gameObject);
			waypoint = null;
		}
	}

	/**********************************************************/
	// Accessors

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

	public bool Available
	{
		get
		{
			return available;
		}
		set
		{
			available = value;
		}
	}

	public float RespawnTime
	{
		get
		{
			return respawnTime;
		}
		set
		{
			respawnTime = value;
		}
	}

	public bool Respawns
	{
		get
		{
			return respawns;
		}
		set
		{
			respawns = value;
		}
	}
}
