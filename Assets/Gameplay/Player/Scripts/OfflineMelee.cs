using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OfflineMelee : MonoBehaviour
{
	[SerializeField]
	protected float range;
	[SerializeField]
	protected float facingThreshold;
	[SerializeField]
	protected float checkHitDuration;

	protected float damage;
	protected float rate;

	protected float activeTime;
	protected bool canMelee;
	protected bool bufferedInput;

	protected OfflineWeaponCarrier weapon;
	protected OfflineGrenadeCarrier grenades;
	protected OfflinePlayerModel model;
	protected CameraManager cam;
	protected HUD hud;
	protected MeshRenderer knife;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		damage = 66.67f;
		rate = 2.0f;

		canMelee = true;

		weapon = GetComponent<OfflineWeaponCarrier>();
		grenades = GetComponent<OfflineGrenadeCarrier>();
		model = GetComponent<OfflinePlayerModel>();
		cam = GetComponentInChildren<CameraManager>();
		hud = GameObject.Find("HUD").GetComponent<HUD>();
		knife = Utility.FindChild(model.FirstPersonHands.gameObject, "Knife").GetComponent<MeshRenderer>();
	}

	public virtual void Update()
	{
		activeTime -= Time.deltaTime;
		if (!Active)
		{
			KnifeVisible = false;
		}

		if (PlayerInput.Melee(ButtonStatus.Pressed))
		{
			bufferedInput = true;
		}

		if (bufferedInput && canMelee && grenades.CanThrow)
		{
			Melee();
			bufferedInput = false;
		}
	}

	/**********************************************************/
	// Interface

	protected virtual void Melee()
	{
		activeTime = (1.0f / rate);
		canMelee = false;

		model.OnMelee();

		KnifeVisible = true;
	}

	public virtual void OnMeleeReady()
	{
		canMelee = true;
	}

	public virtual void OnMeleeDone()
	{
		weapon.OnMeleeDone();
	}

	public virtual void OnMeleeHit()
	{
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Active
	{
		get
		{
			return activeTime > 0.0f;
		}
	}

	public bool KnifeVisible
	{
		get
		{
			return knife.enabled;
		}
		set
		{
			foreach (MeshRenderer m in knife.GetComponentsInChildren<MeshRenderer>())
			{
				m.enabled = value;
			}

			if (!value)
			{
				canMelee = true;
			}
		}
	}
}