using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour
{
	[SerializeField]
	private int order;
	[SerializeField]
	private GameObject waypointPrefab;

	private int[] numPlayers;
	private List<int>[] playersInPoint;

	private BoxCollider col;
	private MeshRenderer[] meshes;
	private Transform waypointTransform;
	private CapturePointWaypoint waypoint;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		numPlayers = new int[2];
		playersInPoint = new List<int>[2];
		for (int i = 0; i < 2; i++)
		{
			playersInPoint[i] = new List<int>();
			for (int j = 0; j < 8; j++)
			{
				playersInPoint[i].Add(0);
			}
		}

		col = GetComponent<BoxCollider>();
		meshes = GetComponentsInChildren<MeshRenderer>();
		waypointTransform = transform.Find("Waypoint");

		enabled = false;
	}

	public void OnEnable()
	{
		Enable(true);

		waypoint = Instantiate(waypointPrefab).GetComponent<CapturePointWaypoint>();
		waypoint.Camera = PlayerManager.LocalPlayer.Cam.WorldCamera;

		Utility.SetRGB(meshes[0].material, Color.white);
		for (int i = 1; i < meshes.Length; i++)
		{
			meshes[i].material = meshes[0].material;
		}
	}

	public void OnDisable()
	{
		Enable(false);

		if (waypoint)
		{
			Destroy(waypoint.gameObject);
			waypoint = null;
		}
	}

	public void LateUpdate()
	{
		waypoint.WorldPosition = waypointTransform.position;
	}

	public void FixedUpdate()
	{
		for (int i = 0; i < numPlayers.Length; i++)
		{
			numPlayers[i] = 0;
		}
	}

	public void OnTriggerStay(Collider col)
	{
		NetworkPlayer player = col.GetComponent<NetworkPlayer>();
		if (player && !player.Respawner.enabled)
		{
			playersInPoint[player.Team][numPlayers[player.Team]] = player.ID;
			numPlayers[player.Team]++;
		}
	}

	/**********************************************************/
	// Helper Functions

	private void Enable(bool enable)
	{
		col.enabled = enable;
		foreach (MeshRenderer mesh in meshes)
		{
			mesh.enabled = enable;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int Order
	{
		get
		{
			return order;
		}
		set
		{
			order = value;
		}
	}

	public int[] NumPlayers
	{
		get
		{
			return numPlayers;
		}
	}

	public List<int>[] PlayersInPoint
	{
		get
		{
			return playersInPoint;
		}
	}

	public float CaptureAmount
	{
		set
		{
			waypoint.CaptureAmount = value;
			if (value > 0.5f)
			{
				Utility.SetRGB(meshes[0].material, Color.Lerp(Color.white, PartyManager.GetTeamColor(0), (value - 0.5f) / 0.5f));
			}
			else
			{
				Utility.SetRGB(meshes[0].material, Color.Lerp(PartyManager.GetTeamColor(1), Color.white, value / 0.5f));
			}
		}
	}
}