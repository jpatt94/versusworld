using UnityEngine;
using System.Collections;

public class WeaponSpawner : MonoBehaviour
{
	[SerializeField]
	private WeaponType type;
	[SerializeField]
	private bool spawnOnStart;
	[SerializeField]
	private float spawnTime;
	[SerializeField]
	private bool hasWaypoint;

	private float spawnTimeRemaining;
	private bool readyToSpawn;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Start()
	{
		foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
		{
			m.enabled = false;
		}

		spawnTimeRemaining = spawnTime;
		readyToSpawn = false;
		enabled = false;
	}

	public void Update()
	{
		spawnTimeRemaining -= Time.deltaTime;
		if (spawnTimeRemaining <= 0.0f)
		{
			spawnTimeRemaining = 0.0f;
			enabled = false;
			readyToSpawn = true;
		}
	}

	/**********************************************************/
	// Interface

	public WeaponType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public bool SpawnOnStart
	{
		get
		{
			return spawnOnStart;
		}
	}

	public void Enable()
	{
		spawnTimeRemaining = spawnTime;
		enabled = true;
		readyToSpawn = false;
	}

	public bool ReadyToSpawn
	{
		get
		{
			return readyToSpawn;
		}
		set
		{
			readyToSpawn = value;
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
