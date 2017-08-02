using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkPowerUpCarrier : OfflinePowerUpCarrier
{
	[SerializeField]
	private GameObject grenadeCloudPositionerPrefab;

	private PowerUpType waitingForUse;
	private bool canUse;
	private PowerUpType currentTraitsPowerUp;
	private float traitsPowerUpTime;

	private PowerUpManager mgr;
	private GrenadeCloudPositioner grenadeCloudPositioner;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Update()
	{
		if (isServer)
		{
			if (currentTraitsPowerUp != PowerUpType.None)
			{
				traitsPowerUpTime -= Time.deltaTime;
				if (traitsPowerUpTime <= 0.0f)
				{
					StopTraitsPowerUp(currentTraitsPowerUp);
				}
			}
		}

		if (!net.Initialized || !hasAuthority)
		{
			return;
		}

		if (PlayerInput.UsePowerUp(ButtonStatus.Pressed) && waitingForUse != PowerUpType.None && canUse)
		{
			switch (waitingForUse)
			{
				case PowerUpType.GrenadeCloud: EnableGrenadeCloudPositioner(); break;
				case PowerUpType.BigHeads: CmdUseBigHeads(); break;
				case PowerUpType.DamageResist: CmdUseTraitsPowerUp(PowerUpType.DamageResist); break;
				case PowerUpType.SpeedBoost: CmdUseTraitsPowerUp(PowerUpType.SpeedBoost); break;
				case PowerUpType.DamageBoost: CmdUseTraitsPowerUp(PowerUpType.DamageBoost); break;
			}

			waitingForUse = PowerUpType.None;
			hud.PowerUpSpinner.Use();
		}
	}

	/**********************************************************/
	// Interface

	public void DelayedAwake()
	{
		waitingForUse = PowerUpType.None;
		currentTraitsPowerUp = PowerUpType.None;

		mgr = GameObject.Find("PowerUpManager").GetComponent<PowerUpManager>();

		JP.Event.Register(this, "OnPowerUpSpinnerDone");
	}

	protected override void OnPowerUpCollide(PowerUp other)
	{
		if (net.hasAuthority && other.Available && waitingForUse == PowerUpType.None)
		{
			CmdPickUpPowerUp(other.ID);
		}
	}

	public void OnPowerUpSpinnerDone()
	{
		if (waitingForUse != PowerUpType.None)
		{
			canUse = true;
		}
	}

	public void OnDeath()
	{
		waitingForUse = PowerUpType.None;

		if (grenadeCloudPositioner)
		{
			Destroy(grenadeCloudPositioner.gameObject);
			grenadeCloudPositioner = null;
		}

		if (isServer)
		{
			StopTraitsPowerUp(currentTraitsPowerUp);
		}
	}

	public void OnGrenadeCloudPositionerClick(Vector3 position)
	{
		CmdUseGrenadeCloud(position);

		Destroy(grenadeCloudPositioner.gameObject);
		grenadeCloudPositioner = null;
	}

	public void OnDestroy()
	{
		JP.Event.Unregister(this, "OnPowerUpSpinnerDone");
	}

	public void StartTraitsPowerUp(PowerUpType type)
	{
		currentTraitsPowerUp = type;
		net.Traits = (PlayerTraitsType)((int)PlayerTraitsType.DamageResist + (type - PowerUpType.DamageResist));
		net.Model.SetPowerUpMaterial(type);

		if (isServer && currentTraitsPowerUp != PowerUpType.None)
		{
			RpcStartTraitsPowerUp(type);
		}
	}

	public void StopTraitsPowerUp(PowerUpType type)
	{
		currentTraitsPowerUp = PowerUpType.None;
		net.Traits = PlayerTraitsType.Default;
		net.Model.SetPowerUpMaterial(PowerUpType.None);

		if (isServer)
		{
			RpcStopTraitsPowerUp(type);
		}
	}

	/**********************************************************/
	// Commands

	[Command]
	private void CmdPickUpPowerUp(int id)
	{
		mgr.PickUp(net, id);
	}

	[Command]
	private void CmdUseGrenadeCloud(Vector3 position)
	{
		mgr.UseGrenadeCloud(net, position);
	}

	[Command]
	private void CmdUseBigHeads()
	{
		mgr.UseBigHeads(net);
	}

	[Command]
	private void CmdUseTraitsPowerUp(PowerUpType type)
	{
		mgr.UseTraitsPowerUp(net, type);
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	public void RpcOnPickUpPowerUp(PowerUpType type)
	{
		if (hasAuthority)
		{
			if (PowerUpManager.PowerUpNeedsActivation(type))
			{
				waitingForUse = type;
				canUse = false;
			}

			hud.PowerUpSpinner.Spin(type, PowerUpManager.PowerUpNeedsActivation(type));

			print("Awarded power up of type " + type.ToString());
		}
	}

	[ClientRpc]
	private void RpcStartTraitsPowerUp(PowerUpType type)
	{
		if (!isServer)
		{
			StartTraitsPowerUp(type);
		}
	}

	[ClientRpc]
	private void RpcStopTraitsPowerUp(PowerUpType type)
	{
		if (!isServer)
		{
			StopTraitsPowerUp(type);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void EnableGrenadeCloudPositioner()
	{
		GameObject obj = Instantiate(grenadeCloudPositionerPrefab);
		obj.transform.SetParent(hud.StaticCanvas.transform, false);

		grenadeCloudPositioner = obj.GetComponent<GrenadeCloudPositioner>();
		grenadeCloudPositioner.Carrier = this;
		grenadeCloudPositioner.CoverageDiameter = mgr.Settings.GrenadeCloud.Size;
	}

	/**********************************************************/
	// Accessors/Mutators

	public float TraitsPowerUpTime
	{
		get
		{
			return traitsPowerUpTime;
		}
		set
		{
			traitsPowerUpTime = value;
		}
	}
}
