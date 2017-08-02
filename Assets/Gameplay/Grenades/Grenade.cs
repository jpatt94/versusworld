using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grenade : NetworkBehaviour
{
	protected int id;
	protected float fuseTime;
	protected bool detonated;

	protected GrenadeManager mgr;
	protected GrenadeData data;

	protected AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		aud = GetComponent<AudioSource>();
	}

	public virtual void Update()
	{
		fuseTime -= Time.deltaTime;
		if (isServer)
		{
			if (IsFuseTimeExpired())
			{
				Detonate();
			}
		}
	}

	/**********************************************************/
	// Interface

	public virtual GrenadeType GetGrenadeType()
	{
		return GrenadeType.None;
	}

	public virtual void Detonate()
	{
		detonated = true;
		mgr.OnDetonate(id);
	}

	/**********************************************************/
	// Helper Functions

	protected virtual bool IsFuseTimeExpired()
	{
		return fuseTime <= 0.0f;
	}

	/**********************************************************/
	// Accessors/Mutators

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

	public float FuseTime
	{
		get
		{
			return fuseTime;
		}
		set
		{
			fuseTime = value;
		}
	}

	public int Thrower
	{
		get
		{
			return data.throwerID;
		}
		set
		{
			data.throwerID = value;
		}
	}
}