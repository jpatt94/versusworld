using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMelee : OfflineMelee
{
	private NetworkPlayer target;

	private NetworkPlayer net;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		net = GetComponent<NetworkPlayer>();
		knife = Utility.FindChild(model.ThirdPersonModel.gameObject, "Knife").GetComponent<MeshRenderer>();
	}

	public override void Update()
	{
		base.Update();
	}

	/**********************************************************/
	// Interface

	public void DelayedAwake()
	{
		Traits = PlayerTraitsType.Default;
	}

	public void OnStartAuthority()
	{
		knife = Utility.FindChild(model.FirstPersonHands.gameObject, "Knife").GetComponent<MeshRenderer>();
	}

	protected override void Melee()
	{
		if (net.hasAuthority)
		{
			base.Melee();
			target = null;
			CheckHit();
		}
	}

	public override void OnMeleeHit()
	{
		base.OnMeleeHit();

		//RaycastHit hit;
		//Physics.Raycast(cam.FirstPersonCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.4f)), out hit, range, NetworkPlayer.BodyPartLayerMask);
		//if (hit.collider)
		//{
		//	BodyPartCollider bodyPart = hit.collider.GetComponent<BodyPartCollider>();
		//	if (bodyPart)
		//	{
		//		NetworkPlayer player = bodyPart.GetComponentInParent<NetworkPlayer>();
		//		net.CmdMelee(player.ID, hit.point, hit.normal, bodyPart.type);
		//
		//		hud.HitMarker.Trigger(HitMarkerType.Default);
		//		CreateHitEffects(hit.point, (cam.transform.position - hit.point).normalized, bodyPart.type);
		//	}
		//}

		if (target)
		{
			Vector3 normal = (cam.transform.position - target.Center).normalized;
			net.Melee(target.ID, target.Center, normal, BodyPart.UpperTorso);
			hud.HitMarker.Trigger(PartyManager.SameTeam(net.ID, target.ID) ? HitMarkerType.Friendly : HitMarkerType.Default);
			CreateHitEffects(target.Center, normal, BodyPart.UpperTorso);
		}
	}

	public void CreateHitEffects(Vector3 position, Vector3 normal, BodyPart bodyPart)
	{
		GameObject obj = Instantiate(EnvironmentManager.GetPlayerHitEffect(0));
		obj.transform.position = position;
		obj.transform.forward = normal;
	}

	/**********************************************************/
	// Helper Functions

	private bool CheckHit()
	{
		RaycastHit[] hits = Physics.SphereCastAll(cam.transform.position, range, cam.transform.forward, 0.0f, 1 << LayerMask.NameToLayer("PlayerController"));
		List<NetworkPlayer> eligiblePlayers = null;
		for (int i = 0; i < hits.Length; i++)
		{
			NetworkPlayer otherNet = hits[i].collider.GetComponent<NetworkPlayer>();
			if (otherNet && net != otherNet && Vector3.Dot(cam.transform.forward, (otherNet.Center - cam.transform.position).normalized) >= facingThreshold)
			{
				if (eligiblePlayers == null)
				{
					eligiblePlayers = new List<NetworkPlayer>();
				}
				eligiblePlayers.Add(otherNet);
			}
		}

		if (eligiblePlayers != null)
		{
			int closestIndex = 0;
			float closestDot = Vector3.Dot(cam.transform.forward, (eligiblePlayers[0].Center - cam.transform.position).normalized);
			for (int i = 1; i < eligiblePlayers.Count; i++)
			{
				float dot = Vector3.Dot(cam.transform.forward, (eligiblePlayers[i].Center - cam.transform.position).normalized);
				if (dot > closestDot)
				{
					closestIndex = i;
					closestDot = dot;
				}
			}

			target = eligiblePlayers[closestIndex];
			return true;
		}

		return false;
	}

	/**********************************************************/
	// Accessors/Mutators

	public PlayerTraitsType Traits
	{
		set
		{
			MeleeSettings settings = PartyManager.GameSettings.GetPlayerTraits(value).Melee;

			damage = settings.Damage;
			rate = settings.Rate;

			net.Model.FirstPersonHands.Animator.SetFloat("MeleeSpeed", rate);
			net.Model.ThirdPersonModel.Animator.SetFloat("MeleeSpeed", rate);
		}
	}
}