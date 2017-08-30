using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GrenadeCloud : NetworkBehaviour
{
	[SerializeField]
	private float outroDuration;
	[SerializeField]
	private AnimationCurve outroScaleCurve;

	[SyncVar]
	private int ownerID;
	[SyncVar]
	private Vector3 velocity;
	[SyncVar]
	private float duration;

	private float time;
	private GrenadeCloudState state;
	private float nextSpawn;

	private GrenadeManager mgr;
	private GrenadeCloudSettings settings;
	private Material mat;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public void Awake()
	{
		mgr = GameObject.Find("GrenadeManager").GetComponent<GrenadeManager>();
		settings = PartyManager.GameSettings.PowerUps.GrenadeCloud;
		mat = GetComponentInChildren<MeshRenderer>().material;

		transform.localScale = Vector3.one * settings.Size;
		state = GrenadeCloudState.Intro;
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		GameObject.Find("HUD").GetComponent<HUD>().KillFeed.ShowMessage(PlayerManager.GetPlayer(ownerID).Name + ": Cloudy with a chance of grenades", ownerID == PlayerManager.LocalPlayerID ? 1 : 0);
	}

	public void Update()
	{
		time += Time.deltaTime;
		transform.position += velocity * Time.deltaTime;

		switch (state)
		{
			case GrenadeCloudState.Intro: UpdateIntro(); break;
			case GrenadeCloudState.Raining: UpdateRaining(); break;
			case GrenadeCloudState.Outro: UpdateOutro(); break;
		}
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateIntro()
	{
		mat.color = new Color(0.0f, 0.0f, 0.0f, time / settings.IntroDuration);

		if (time >= settings.IntroDuration)
		{
			time = 0.0f;
			state = GrenadeCloudState.Raining;
		}
	}

	private void UpdateRaining()
	{
		if (isServer)
		{
			nextSpawn -= Time.deltaTime;
			if (nextSpawn <= 0.0f)
			{
				CreateGrenade();
				nextSpawn = Mathf.Lerp(settings.MinimumSpawnTime, settings.MaximumSpawnTime, Random.value);
			}
		}

		if (time >= duration)
		{
			time = 0.0f;
			state = GrenadeCloudState.Outro;
		}
	}

	private void UpdateOutro()
	{
		mat.color = new Color(0.0f, 0.0f, 0.0f, 1.0f - time / outroDuration);
		transform.localScale = Vector3.one * settings.Size * outroScaleCurve.Evaluate((1.0f - time / outroDuration));

		if (isServer)
		{
			if (time >= outroDuration)
			{
				NetworkServer.UnSpawn(gameObject);
				Destroy(gameObject);
			}
		}
	}

	private void CreateGrenade()
	{
		Vector3 position = transform.position;
		Vector2 rand = Random.insideUnitCircle;
		position.x += rand.x * (settings.Size * 0.5f);
		position.z += rand.y * (settings.Size * 0.5f);

		Grenade g = mgr.CreateLiveGrenade(settings.GrenadeType, position, Vector3.down, 0.0f, ownerID, false);

		g.FuseTime = settings.GrenadeFuseTime;
	}

	/**********************************************************/
	// Accessors/Mutators

	public int OwnerID
	{
		get
		{
			return ownerID;
		}
		set
		{
			ownerID = value;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			return velocity;
		}
		set
		{
			velocity = value;
		}
	}

	public float Duration
	{
		get
		{
			return duration;
		}
		set
		{
			duration = value;
		}
	}
}

public enum GrenadeCloudState
{
	Intro,
	Raining,
	Outro,
}