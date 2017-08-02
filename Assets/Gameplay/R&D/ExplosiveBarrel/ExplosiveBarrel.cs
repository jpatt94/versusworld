using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExplosiveBarrel : Damageable
{
	[SerializeField]
	private float damage;
	[SerializeField]
	private float radius;
	[SerializeField]
	private GameObject explosionPrefab;
	[SerializeField]
	private GameObject spawnerPrefab;

	/**********************************************************/
	// Interface

	public override float TakeDamage(float damage, int shooter, Vector3 position, DamageType type)
	{
		if (isServer && health > 0.0f)
		{
			base.TakeDamage(damage, shooter, position, type);

			if (health <= 0.0f)
			{
				GameObject.Find("WeaponManager").GetComponent<WeaponManager>().DealExplosiveDamage(shooter, gameObject, this.damage, radius, DamageType.ExplosiveBarrel);

				GameObject obj = Instantiate(spawnerPrefab);
				Spawner spawner = obj.GetComponent<Spawner>();
				spawner.transform.position = transform.position;
				spawner.transform.rotation = transform.rotation;

				NetworkServer.UnSpawn(gameObject);
				Destroy(gameObject);
			}
		}

		return damage;
	}

	public override void OnNetworkDestroy()
	{
		base.OnNetworkDestroy();

		Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 3.0f);
	}
}