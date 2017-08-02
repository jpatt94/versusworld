using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
	[SerializeField]
	private int initialSpawnIndex;
	[SerializeField]
	private int teamIndex;
	[SerializeField]
	private int initialSpawnTeamIndex;

	private Vector3 top;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		top = transform.position + Vector3.up * 2.0f;
	}

	public void Start()
	{
		foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
		{
			mesh.enabled = false;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int InitialSpawnIndex
	{
		get
		{
			return initialSpawnIndex;
		}
	}

	public int TeamIndex
	{
		get
		{
			return teamIndex;
		}
	}

	public int InitialSpawnTeamIndex
	{
		get
		{
			return initialSpawnTeamIndex;
		}
	}

	public Vector3 Top
	{
		get
		{
			return top;
		}
	}
}
