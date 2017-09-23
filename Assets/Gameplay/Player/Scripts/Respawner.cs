using UnityEngine;
using System.Collections;

public class Respawner : MonoBehaviour
{
	private Vector3 respawnPosition;
	private Quaternion respawnRotation;
	private float respawnTime;
	private float time;

	private NetworkPlayer net;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Start()
	{
		time = 0.0f;

		net = GetComponent<NetworkPlayer>();

		enabled = false;
	}

	public void Update()
	{
		if (net.isServer && !net.Party.Game.IsGameOver)
		{
			time += Time.deltaTime;
			if (time >= respawnTime)
			{
				net.Respawn();
				enabled = false;
			}
		}
	}

	/**********************************************************/
	// Interface

	public void StartRespawn(Vector3 pos, Quaternion rot)
	{
		print("Starting respawn");
		if (net.isServer)
		{
			respawnPosition = pos;
			respawnRotation = rot;
			time = 0.0f;
			enabled = true;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public RespawnSettings Traits
	{
		set
		{
			respawnTime = value.Time;
		}
	}

	public Vector3 RespawnPosition
	{
		get
		{
			return respawnPosition;
		}
	}

	public Quaternion RespawnQuaternion
	{
		get
		{
			return respawnRotation;
		}
	}
}
