using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GrenadeDrop : NetworkBehaviour
{
	[SerializeField]
	private GrenadeType type;
	[SerializeField]
	private bool despawns;
	[SerializeField]
	private float despawnTime;

	[SyncVar]
	private int id;

	private GrenadeData data;
	private GrenadeManager mgr;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		GetComponent<Rigidbody>().maxDepenetrationVelocity = 3.0f;
	}

	public void Update()
	{
		if (isServer && despawns)
		{
			despawnTime -= Time.deltaTime;
			if (despawnTime <= 0.0f)
			{
				Manager.DespawnGrenade(this);
			}
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public GrenadeType Type
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

	public GrenadeData Data
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

	public GrenadeManager Manager
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
}
