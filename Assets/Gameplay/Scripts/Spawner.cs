using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : MonoBehaviour
{
	[SerializeField]
	private float spawnTime;
	[SerializeField]
	private GameObject spawnObject;
	[SerializeField]
	private bool spawnOnNetwork;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Update()
	{
		spawnTime -= Time.deltaTime;
		if (spawnTime <= 0.0f)
		{
			GameObject obj = Instantiate(spawnObject);
			obj.transform.position = transform.position;
			obj.transform.rotation = transform.rotation;

			if (spawnOnNetwork)
			{
				NetworkServer.Spawn(obj);
			}

			Destroy(gameObject);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public float SpawnTime
	{
		get
		{
			return spawnTime;
		}
		set
		{
			spawnTime = value;
		}
	}

	public GameObject SpawnObject
	{
		get
		{
			return spawnObject;
		}
		set
		{
			spawnObject = value;
		}
	}
}