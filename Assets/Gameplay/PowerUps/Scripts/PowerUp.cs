using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PowerUp : NetworkBehaviour
{
	[SerializeField]
	private GameObject waypointPrefab;
	[SerializeField]
	private AudioClip respawnSound;
	[SerializeField]
	private AudioClip pickUpSound;

	[SyncVar]
	private int id;
	[SyncVar(hook = "OnAvailableChanged")]
	private bool available;

	private float respawnTime;
	private bool respawns;

	private MeshRenderer mesh;
	private Waypoint waypoint;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		id = -1;
		available = false;

		mesh = GetComponentInChildren<MeshRenderer>();
		aud = GetComponent<AudioSource>();
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
	// Callbacks

	private void OnAvailableChanged(bool value)
	{
		bool prev = available;
		available = value;

		if (!prev && available)
		{
			OnRespawn();
		}
		else if (prev == !available)
		{
			OnPickUp();
		}
	}

	/**********************************************************/
	// Helper Functions

	private void OnRespawn()
	{
		aud.PlayOneShot(respawnSound);
	}

	private void OnPickUp()
	{
		aud.PlayOneShot(pickUpSound);
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
