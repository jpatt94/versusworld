using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragDamageable : Damageable
{
	/**********************************************************/
	// Interface

	public override float TakeDamage(float damage, int shooter, Vector3 position, DamageType type)
	{
		if (type == DamageType.FragGrenade || type == DamageType.FragGrenadeShot ||
			type == DamageType.TeslaGrenade || type == DamageType.RocketLauncher ||
			type == DamageType.SpikeGrenade || type == DamageType.ExplosiveBarrel)
		{
			return 0.0f;
		}

		float retval = base.TakeDamage(damage, shooter, position, type);

		if (isServer && health <= 0.0f)
		{
			Frag frag = GetComponent<Frag>();
			frag.Thrower = shooter;
			frag.ShotDetonated = true;
			frag.Detonate();
		}

		return retval;
	}
}