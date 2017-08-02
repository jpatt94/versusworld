using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Rocket : NetworkBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab;
	[SerializeField]
	private GameObject smokePrefab;

	private int shooterID;
	private float speed;
	private float acceleration;
	private float time;
	private bool detonated;
	private Vector3 prevPosition;

	private WeaponManager mgr;
	private Rigidbody rig;
	private GameObject smoke;

	/**********************************************************/
	// Interface

	public void Awake()
	{
		shooterID = -1;

		RocketLauncherSettings settings = PartyManager.GameSettings.Weapons.RocketLauncher;
		speed = settings.InitialSpeed;
		acceleration = settings.Acceleration;
		time = 0.0f;

		rig = GetComponent<Rigidbody>();
		smoke = Instantiate(smokePrefab);
	}

	public void Update()
	{
		if (mgr && mgr.isServer)
		{
			time += Time.deltaTime;
			if (time >= 10.0f)
			{
				mgr.OnRocketHit(this, false, transform.position);
			}
		}
	}

	public void FixedUpdate()
	{
		speed += acceleration * Time.fixedDeltaTime;

		rig.velocity = transform.forward * speed;

		smoke.transform.position = transform.position;
		smoke.transform.rotation = transform.rotation;

		prevPosition = rig.position;
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (mgr && mgr.isServer && !detonated)
		{
			bool hit = true;

			BodyPartCollider bodyPart = collider.GetComponent<BodyPartCollider>();
			if (bodyPart)
			{
				if (bodyPart.GetComponentInParent<NetworkPlayer>().ID == shooterID)
				{
					hit = false;
				}
			}
			else if (collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") || collider.gameObject.layer == LayerMask.NameToLayer("PlayerController"))
			{
				hit = false;
			}

			if (hit)
			{
				Vector3 delta = rig.position - prevPosition;
				RaycastHit raycast;
				Physics.Raycast(new Ray(prevPosition, delta.normalized), out raycast, delta.magnitude, NetworkPlayer.BodyPartLayerMask);

				detonated = true;
				mgr.OnRocketHit(this, bodyPart != null, raycast.collider ? raycast.point : transform.position);
			}
		}
	}

	public void OnDestroy()
	{
		ParticleSystem.EmissionModule em = smoke.GetComponent<ParticleSystem>().emission;
		em.rateOverTime = 0.0f;
		Destroy(smoke, 5.0f);

		Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 3.0f);
	}

	/**********************************************************/
	// Accessors/Mutators

	public int ShooterID
	{
		get
		{
			return shooterID;
		}
		set
		{
			shooterID = value;
		}
	}

	public WeaponManager Mgr
	{
		get
		{
			return mgr;
		}
		set
		{
			mgr = value;
		}
	}
}