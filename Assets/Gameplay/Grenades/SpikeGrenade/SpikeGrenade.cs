using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpikeGrenade : Grenade
{
	[SerializeField]
	private float spikesInScale;
	[SerializeField]
	private float spikesOutScale;
	[SerializeField]
	private float showSpikesTime;
	[SerializeField]
	private float showSpikesDuration;
	[SerializeField]
	private GameObject stuckPrefab;
	[SerializeField]
	private AudioClip spikesOutSound;

	private float time;
	private bool needsSpikesOutSound;
	private Vector3 prevPos;
	private float radius;
	private SpikeGrenadeStuck stuck;
	private int stuckPlayer;

	private Transform spikesTransform;
	private Rigidbody rig;
	private SpikeSettings settings;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		settings = PartyManager.GameSettings.Grenades.Spike;

		spikesTransform = transform.Find("Body/Spikes");
		spikesTransform.localScale = Vector3.one * spikesInScale;

		rig = GetComponent<Rigidbody>();

		time = 0.0f;
		needsSpikesOutSound = true;
		prevPos = transform.position;
		radius = GetComponent<SphereCollider>().radius;

		FuseTime = float.MaxValue;
	}

	public override void Update()
	{
		base.Update();

		time += Time.deltaTime;
		if (time > showSpikesTime)
		{
			if (needsSpikesOutSound)
			{
				aud.PlayOneShot(spikesOutSound);
				needsSpikesOutSound = false;
			}

			spikesTransform.localScale = Vector3.one * Mathf.Lerp(spikesInScale, spikesOutScale, (time - showSpikesTime) / (showSpikesDuration - showSpikesTime));
		}
	}

	public void FixedUpdate()
	{
		if (isServer)
		{
			if (stuck == null)
			{
				Vector3 vel = transform.position - prevPos;
				RaycastHit[] hits = Physics.SphereCastAll(new Ray(prevPos, vel.normalized), radius, vel.magnitude, ~((1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("PlayerController"))));
				foreach (RaycastHit hit in hits)
				{
					BodyPartCollider bpc = hit.collider.GetComponent<BodyPartCollider>();
					if (bpc)
					{
						int stuckPlayer = bpc.GetComponentInParent<NetworkPlayer>().ID;
						if (stuckPlayer != Thrower)
						{
							CreateStuckObject(bpc.transform.InverseTransformPoint(hit.point), transform.rotation, stuckPlayer, bpc.type);
							break;
						}
					}
				}

				prevPos = transform.position;
			}
			else
			{
				rig.position = stuck.transform.position;
			}
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (isServer && stuck == null)
		{
			CreateStuckObject(transform.position, transform.rotation, -1, BodyPart.None);
		}
	}

	public void OnDestroy()
	{
		if (mgr && mgr.StuckSpikeGrenades.Contains(this))
		{
			mgr.StuckSpikeGrenades.Remove(this);
		}

		if (stuck)
		{
			Destroy(stuck.gameObject);
		}
	}

	/**********************************************************/
	// Interface

	public override GrenadeType GetGrenadeType()
	{
		return GrenadeType.Spike;
	}

	public override void Detonate()
	{
		if (!detonated)
		{
			detonated = true;
			mgr.DealExplosiveDamage(data.throwerID, gameObject, settings.Damage, settings.Range, DamageType.SpikeGrenade);

			base.Detonate();
		}
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcStick(Vector3 position, Quaternion rotation, int playerStuck, BodyPart bodyPart)
	{
		if (!isServer)
		{
			CreateStuckObject(position, rotation, playerStuck, bodyPart);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void CreateStuckObject(Vector3 position, Quaternion rotation, int playerStuck, BodyPart bodyPart)
	{
		GameObject obj = Instantiate(stuckPrefab);
		stuck = obj.GetComponent<SpikeGrenadeStuck>();
		stuck.FuseTime = settings.FuseTime;

		if (bodyPart == BodyPart.None)
		{
			stuck.transform.position = position;
		}
		else
		{
			NetworkPlayer p = PlayerManager.GetPlayer(playerStuck);
			stuck.transform.SetParent(p.Model.BodyPartTransforms[bodyPart]);
			stuck.transform.localPosition = position;

			if (isServer)
			{
				PlayerManager.GetPlayer(Thrower).DealDamage(p, settings.StickDamage, position, bodyPart, DamageType.SpikeGrenade);
				PlayerManager.GetPlayer(Thrower).MarkHit(PartyManager.SameTeam(Thrower, p.ID) ? HitMarkerType.Friendly : HitMarkerType.Default);
			}

			GameObject.Find("HUD").GetComponent<HUD>().KillFeed.ShowMessage(PlayerManager.GetPlayer(playerStuck).Name + ": Goodbye cruel world!", Color.white);
		}
		stuck.transform.rotation = rotation;

		stuckPlayer = playerStuck;
		if (mgr && stuckPlayer > -1)
		{
			mgr.StuckSpikeGrenades.Add(this);
		}

		foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
		{
			m.enabled = false;
		}
		GetComponent<TrailRenderer>().enabled = false;

		rig.isKinematic = true;

		if (isServer)
		{
			fuseTime = settings.FuseTime;
			RpcStick(position, rotation, playerStuck, bodyPart);
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public int StuckPlayer
	{
		get
		{
			return stuckPlayer;
		}
	}
}