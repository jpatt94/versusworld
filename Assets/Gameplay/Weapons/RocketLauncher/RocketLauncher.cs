using UnityEngine;
using System.Collections;

public class RocketLauncher : Weapon
{
	[SerializeField]
	private Vector2 jerk;
	[SerializeField]
	private float jerkDuration;

	/**********************************************************/
	// Weapon Interface

	public override void OnStart()
	{
		base.OnStart();
	}

	public override WeaponType GetWeaponType()
	{
		return WeaponType.RocketLauncher;
	}

	public override void LoadGameSettings()
	{
		RocketLauncherSettings settings = PartyManager.GameSettings.Weapons.RocketLauncher;

		ammo = settings.Ammo;
		clipSize = settings.ClipSize;
		fireRate = settings.FireRate;
		reloadTime = settings.ReloadTime;
	}

	protected override void Shoot()
	{
		base.Shoot();

		cam.Jerk(jerk, jerkDuration, true);
		cam.ZShake(1.0f, jerkDuration);

		if (net)
		{
			RaycastHit hit = GetRaycastHit(cam.transform.forward);
			Vector3 direction = hit.collider ? (hit.point - barrel.transform.position).normalized : cam.transform.forward;

			net.ShootRocket(id, barrel.transform.position, direction);
		}
	}
}