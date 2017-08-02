using UnityEngine;
using System.Collections;

public class NetworkGrenadeCarrier : OfflineGrenadeCarrier
{
	[SerializeField]
	private float pickUpGrenadeDelay;
	[SerializeField]
	private float minForceRot;
	[SerializeField]
	private float maxForceRot;
	[SerializeField]
	private float upThrowLerp;

	private NetworkPlayer net;

	private float canPickUpGrenade;

	/**********************************************************/
	// MonoBehaviour Interface

	void OnTriggerStay(Collider other)
	{
		if (canPickUpGrenade > 0.0f && net.hasAuthority && enabled)
		{
			if (other.tag == "GrenadeDrop")
			{
				print(("Picking up grenade"));
				GrenadeDrop drop = other.GetComponentInParent<GrenadeDrop>();
				net.CmdPickUpGrenade(drop.ID);
				canPickUpGrenade = -pickUpGrenadeDelay;
			}
		}
	}

	/**********************************************************/
	// OfflineGrenadeCarrier Interface

	protected override void OnAwake()
	{
		base.OnAwake();

		net = GetComponent<NetworkPlayer>();
		cam = GetComponentInChildren<CameraManager>();
	}

	protected override void OnStart()
	{
		canThrow = 0.0f;
	}

	protected override void OnUpdate()
	{
		if (net.hasAuthority)
		{
			base.OnUpdate();
			canPickUpGrenade += Time.deltaTime;
		}
	}

	protected override void ThrowGrenade()
	{
		float rot = cam.transform.rotation.eulerAngles.x;
		if (rot > 180.0f)
		{
			rot -= 360.0f;
		}

		net.Throw(currentType, cam.transform.TransformPoint(throwPosition), Vector3.Slerp(cam.transform.forward, Vector3.up, upThrowLerp), Mathf.InverseLerp(minForceRot, maxForceRot, rot));

		base.ThrowGrenade();
	}

	/**********************************************************/
	// Interface

	public void OnStartLocalPlayer()
	{
		SelectFirstGrenade();

		GrenadeSettings settings = PartyManager.GameSettings.Grenades;
		throwRate = settings.ThrowRate;
	}

	public void AddGrenades(GrenadeAssignment grenades)
	{
		AddGrenades(grenades.type, grenades.amount);
	}

	public void OnRespawn()
	{
		SelectFirstGrenade();
	}
}