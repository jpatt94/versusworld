using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Glass : Damageable
{
	[SyncVar]
	private Vector3 scale;

	/**********************************************************/
	// Interface

	public override void Start()
	{
		base.Start();

		transform.localScale = scale;
	}

	public override float TakeDamage(float damage, int shooter, Vector3 position, DamageType type)
	{
		if (isServer && health > 0.0f)
		{
			base.TakeDamage(damage, shooter, position, type);

			if (health <= 0.0f)
			{
				NetworkServer.UnSpawn(gameObject);
				Destroy(gameObject);
			}
		}

		return damage;
	}

	/**********************************************************/
	// Accessors/Mutators

	public Vector3 Scale
	{
		get
		{
			return scale;
		}
		set
		{
			scale = value;
		}
	}
}