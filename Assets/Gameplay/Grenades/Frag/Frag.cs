using UnityEngine;
using System.Collections;

public class Frag : Grenade
{
	[SerializeField]
	private GameObject explosionPrefab;

	private bool madeContact;
	private bool shotDetonated;

	private FragSettings settings;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		madeContact = false;
		shotDetonated = false;

		settings = PartyManager.GameSettings.Grenades.Frag;
		fuseTime = settings.FuseTime;
	}

	public void OnDestroy()
	{
		GameObject obj = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(obj, 1.0f);
	}

	public void OnCollisionEnter(Collision col)
	{
		madeContact = true;
	}

	/**********************************************************/
	// Interface

	public override GrenadeType GetGrenadeType()
	{
		return GrenadeType.Frag;
	}

	public override void Detonate()
	{
		if (!detonated)
		{
			detonated = true;
			mgr.DealExplosiveDamage(data.throwerID, gameObject, settings.Damage, settings.Range, shotDetonated ? DamageType.FragGrenadeShot : DamageType.FragGrenade, friendlyFire);

			base.Detonate();
		}
	}

	/**********************************************************/
	// Helper Functions

	protected override bool IsFuseTimeExpired()
	{
		return madeContact ? fuseTime <= 0.0f : fuseTime <= settings.FuseTime - settings.NoContactFuseTime;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool ShotDetonated
	{
		get
		{
			return shotDetonated;
		}
		set
		{
			shotDetonated = value;
		}
	}
}
