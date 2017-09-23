using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkPowerUpCarrier : OfflinePowerUpCarrier
{
	[SerializeField]
	private GameObject grenadeCloudPositionerPrefab;

	private List<TraitPowerUpEntry> traitPowerUps;
	private PowerUpType waitingForUse;
	private bool canUse;

	private PowerUpManager mgr;
	private GrenadeCloudPositioner grenadeCloudPositioner;

	/**********************************************************/
	// MonoBehaviour Interface

	protected override bool Ready()
	{
		return net.Initialized;
	}

	public override void Awake()
	{
		base.Awake();

		traitPowerUps = new List<TraitPowerUpEntry>();
		waitingForUse = PowerUpType.None;

		JP.Event.Register(this, "OnPowerUpSpinnerDone");
	}

	protected override void DelayedAwake()
	{
		mgr = GameObject.Find("PowerUpManager").GetComponent<PowerUpManager>();
	}

	public override void Update()
	{
		base.Update();

		if (!initialized)
		{
			return;
		}

		for (int i = 0; i < traitPowerUps.Count; i++)
		{
			traitPowerUps[i].Time -= Time.deltaTime;

			if (hasAuthority)
			{
				traitPowerUps[i].Icon.TimerAmount = traitPowerUps[i].Time / traitPowerUps[i].Duration;
			}

			if (isServer && traitPowerUps[i].Time <= 0.0f)
			{
				StopTraitsPowerUp(traitPowerUps[i].Type);
				i--;
			}
		}

		if (!hasAuthority)
		{
			return;
		}

		if (PlayerInput.UsePowerUp(ButtonStatus.Pressed) && waitingForUse != PowerUpType.None && canUse)
		{
			switch (waitingForUse)
			{
				case PowerUpType.GrenadeCloud: CmdUseGrenadeCloud(); break;
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
			for (int i = 0; i < traitPowerUps.Count; i++)
			{
				StopTraitsPowerUp(traitPowerUps[i].Type);
				i--;
			}
		}
	}

	public void OnDestroy()
	{
		JP.Event.Unregister(this, "OnPowerUpSpinnerDone");
	}

	public void StartTraitsPowerUp(PowerUpType type, float time)
	{
		for (int i = 0; i < traitPowerUps.Count; i++)
		{
			if (traitPowerUps[i].Type == type)
			{
				traitPowerUps.RemoveAt(i);
				break;
			}
		}

		TraitPowerUpEntry e = new TraitPowerUpEntry();
		e.Type = type;
		e.Time = time;
		e.Duration = time;
		if (hasAuthority)
		{
			e.Icon = hud.AbilityDisplay.AddAbilityIcon(AbilityType.DamageResist + (type - PowerUpType.DamageResist)) as PassiveAbilityIcon;
			e.Icon.Pop();
		}
		traitPowerUps.Add(e);

		net.AddTraitModifier((PlayerTraitModifiersType)((int)PlayerTraitModifiersType.DamageResist + (type - PowerUpType.DamageResist)));

		if (isServer && type != PowerUpType.None)
		{
			RpcStartTraitsPowerUp(type, time);
		}
	}

	public void StopTraitsPowerUp(PowerUpType type)
	{
		for (int i = 0; i < traitPowerUps.Count; i++)
		{
			if (traitPowerUps[i].Type == type)
			{
				if (hasAuthority)
				{
					hud.AbilityDisplay.RemoveAbilityIcon(AbilityType.DamageResist + (type - PowerUpType.DamageResist));
				}
				traitPowerUps.RemoveAt(i);
				break;
			}
		}

		net.RemoveTraitModifier((PlayerTraitModifiersType)((int)PlayerTraitModifiersType.DamageResist + (type - PowerUpType.DamageResist)));

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
	private void CmdUseGrenadeCloud()
	{
		mgr.UseGrenadeCloud(net);
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
	private void RpcStartTraitsPowerUp(PowerUpType type, float time)
	{
		if (!isServer)
		{
			StartTraitsPowerUp(type, time);
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
		//grenadeCloudPositioner.Carrier = this;
		grenadeCloudPositioner.CoverageDiameter = mgr.Settings.GrenadeCloud.Size;
	}
}

public class TraitPowerUpEntry
{
	public PowerUpType Type;
	public float Time;
	public float Duration;
	public PassiveAbilityIcon Icon;
}