using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkPlayer : SafeNetworkBehaviour
{
	public static int BodyPartLayerMask;
	public static int PlayerControllerMask;

	[SerializeField]
	private GameObject ragdollPrefab;

	[SyncVar]
	private int networkID;
	[SyncVar]
	private string userName;
	[SyncVar]
	private int team;

	private bool awoken;
	private bool receivedStartingWeapons;

	private PlayerTraits traits;
	private PlayerTraitsType traitsType;
	private List<PlayerTraitModifiersType> traitModifierTypes;

	private MultiplayerManager mgr;
	private WeaponManager weapons;
	private GrenadeManager grenades;
	private PowerUpManager powerUps;
	private PlayerManager playerManager;
	private PartyManager party;
	private HUD hud;
	private MultiplayerMap map;
	private NetworkWeaponCarrier weaponCarrier;
	private NetworkGrenadeCarrier grenadeCarrier;
	private NetworkPowerUpCarrier powerUpCarrier;
	private PlayerHealth health;
	private Respawner respawn;
	private PlayerHUDManager playerHUD;
	private CapsuleCollider controller;
	private NetworkCharacterController netController;
	private CameraManager cam;
	private RespawnCamera respawnCam;
	private NetworkPlayerModel mod;
	private Rigidbody rig;
	private NetworkMelee melee;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		BodyPartLayerMask = ~((1 << LayerMask.NameToLayer("LocalBodyPartCollider") | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("PlayerController"))));
		PlayerControllerMask = ((1 << LayerMask.NameToLayer("PlayerController")) | (1 << LayerMask.NameToLayer("Default"))) & ~(1 << LayerMask.NameToLayer("Ignore Raycast"));

		PlayerInput.MouseActive = true;
		PlayerInput.KeyboardActive = false;

		GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();
		weaponCarrier = GetComponent<NetworkWeaponCarrier>();
		grenadeCarrier = GetComponent<NetworkGrenadeCarrier>();
		powerUpCarrier = GetComponent<NetworkPowerUpCarrier>();
		health = GetComponent<PlayerHealth>();
		respawn = GetComponent<Respawner>();
		playerHUD = GetComponent<PlayerHUDManager>();
		controller = GetComponent<CapsuleCollider>();
		netController = GetComponent<NetworkCharacterController>();
		cam = GetComponentInChildren<CameraManager>();
		mod = GetComponent<NetworkPlayerModel>();
		rig = GetComponent<Rigidbody>();
		melee = GetComponent<NetworkMelee>();
	}

	public override void Update()
	{
		base.Update();

		if (initialized)
		{
			if (hasAuthority)
			{
				if (initializedAuthority && receivedStartingWeapons && !ready)
				{
					mgr.GetLocalPlayerData().CmdPlayerObjectReady();
					Destroy(FindObjectOfType<LoadingMapCanvas>().gameObject);

					GameObject obj = GameObject.Find("StartGameCanvas");
					if (obj)
					{
						obj.GetComponent<Canvas>().enabled = true;
					}

					ready = true;
				}

				// Debug
				if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.T) && mgr.LocalUser == null)
				{
					if (Input.GetKeyDown(KeyCode.Alpha1))
					{
						foreach (NetworkPlayer p in PlayerManager.PlayerList)
						{
							if (this != p)
							{
								rig.position = p.transform.position + Vector3.up * 2.0f;
								GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();
							}
						}
					}
				}
			}

			if (isServer)
			{
				if (transform.position.y < map.KillY)
				{
					TakeDamage(10000.0f, -1, transform.position, BodyPart.None, DamageType.Nature);
				}
			}
		}
	}

	public void OnDestroy()
	{
		print("Destroying " + this);
		PlayerManager.RemovePlayer(networkID);
	}

	/**********************************************************/
	// Safe Initialization

	protected override bool Ready()
	{
		return GameObject.Find("PlayerManager") && GameObject.Find("MultiplayerMap") && GameObject.Find("HUD").GetComponent<HUD>() &&
			GameObject.Find("PowerUpManager");
	}

	protected override void DelayedAwake()
	{
		if (!GameObject.Find("PlayerManager"))
		{
			Debug.LogError("No player manager found!!!");
		}
		map = GameObject.Find("MultiplayerMap").GetComponent<MultiplayerMap>();
		hud = GameObject.Find("HUD").GetComponent<HUD>();
		respawnCam = GameObject.Find("RespawnCamera").GetComponent<RespawnCamera>();

		melee.DelayedAwake();

		MultiplayerManager.Stats.RegisterPlayer(networkID);

		traits = new PlayerTraits();
		traitModifierTypes = new List<PlayerTraitModifiersType>();
		Traits = PlayerTraitsType.Default;

		awoken = true;
	}

	protected override void DelayedOnStartClient()
	{
		PlayerManager.AddPlayer(this);

		playerHUD.CreateNameTag();
		hud.SetPlayerNameTagActive(ID, true);
		health.enabled = true;

		Texture2D texture = new Texture2D(1024, 1024);
		PlayerCustomizer customizer = FindObjectOfType<PlayerCustomizer>();
		PlayerCustomizationOptions options = mgr.GetPlayerData(ID).CustomizationOptions;
		customizer.CreateTexture(texture, options);
		mod.Texture = texture;
		if (options.Mask > -1)
		{
			Instantiate(PlayerCustomizer.MaskPrefabs[options.Mask], mod.ThirdPersonModel.transform.Find(ThirdPersonModel.GetBodyPartTransformPath(BodyPart.Head)));
		}

		mod.PopulateBodyTransformsThirdPerson();
	}

	protected override void DelayedOnStartServer()
	{
		PlayerManager.AddPlayer(this);
		map.SpawnManager.OnSpawn(this);
	}

	protected override void DelayedOnStartAuthority()
	{
		FindObjectOfType<LoadingMapCanvas>().IncreaseProgress();

		PlayerManager.LocalPlayerID = networkID;

		controller.enabled = true;
		cam.Enable(true);
		weaponCarrier.Enable();
		health.enabled = true;
		grenadeCarrier.OnStartLocalPlayer();

		mod.FirstPersonHands.Visible = true;
		mod.FirstPersonLegs.Visible = true;
		mod.ThirdPersonModel.RenderOnlyShadows();
		mod.ThirdPersonModel.transform.localPosition = mod.FirstPersonLegs.transform.localPosition;

		melee.OnStartAuthority();

		CmdRequestStartingWeapons();

		MultiplayerManager.GetLocalPlayerData().NetworkPlayer = this;

		playerHUD.RemoveNameTag();
		hud.Radar.SetVisible(true);

		foreach (BodyPartCollider b in GetComponentsInChildren<BodyPartCollider>())
		{
			b.gameObject.layer = LayerMask.NameToLayer("LocalBodyPartCollider");
		}

		mod.PopulateBodyTransformsFirstPerson();

		initializedAuthority = true;
	}

	/**********************************************************/
	// Interface

	public override string ToString()
	{
		return userName + " (" + networkID + ")";
	}

	[Server]
	public void DealDamage(NetworkPlayer victim, float damage, Vector3 position, BodyPart bodyPart, DamageType type)
	{
		damage *= traits.Weapons.DamageMultiplier;
		victim.TakeDamage(damage, ID, position, bodyPart, type);
	}

	[Server]
	public void TakeDamage(float damage, int shooter, Vector3 position, BodyPart bodyPart, DamageType type)
	{
		damage /= traits.Health.DamageResistance;
		RpcTakeDamage(damage, shooter, position, bodyPart, type);
		if (!(isServer && isClient))
		{
			health.TakeDamage(damage, shooter, position, bodyPart, type);
		}
	}

	[Server]
	public void Die()
	{
		KillData killData;
		killData.shooter = (health.LastShooter == ID && health.LastEnemyShooter != -1) ? health.LastEnemyShooter : health.LastShooter;
		killData.victim = networkID;
		killData.damageType = health.LastDamageType;
		killData.force = health.DamageForce;
		health.PopulateAssisters(out killData.assisters, out killData.assistDamages);

		int[] medals = null;
		if (!Party.Game.IsGameOver)
		{
			MultiplayerManager.Stats.OnKill(killData);
			Party.Game.OnKill(killData);
			medals = PlayerManager.MedalTracker.OnKill(killData);
		}
		if (medals == null)
		{
			medals = new int[0];
		}

		SpawnPoint spawnPoint = map.SpawnManager.StartRespawn(this);
		respawn.StartRespawn(spawnPoint.transform.position, spawnPoint.transform.rotation);

		WeaponManager.DropWeapons(this);
		grenadeCarrier.enabled = false;
		GrenadeManager.DropGrenades(this);
		PowerUpManager.OnDeath(this);
		PowerUpCarrier.OnDeath();

		WeaponAssignment weapons;
		WeaponManager.RequestStartingWeapons(networkID, out weapons);

		RpcDie(spawnPoint.transform.position, spawnPoint.transform.rotation, weapons, killData, medals);

		List<GrenadeAssignment> grenades;
		GrenadeManager.RequestStartingGrenades(networkID, out grenades);
		foreach (GrenadeAssignment g in grenades)
		{
			RpcAddGrenades(g);
		}
	}

	[Server]
	public void AddAmmo(int weaponID, int ammo)
	{
		RpcAddAmmo(weaponID, ammo);
	}

	[Server]
	public void Respawn()
	{
		map.SpawnManager.OnRespawn(this);
		health.ResetHealth();
		RpcOnRespawn();
	}

	[Server]
	public void PickUpWeapon(WeaponType type, int weaponID, int swappingWeaponID, int ammo, int ammoInClip)
	{
		RpcPickUpWeapon(type, weaponID, swappingWeaponID, ammo, ammoInClip);
	}

	[Server]
	public void MarkHit(HitMarkerType type)
	{
		RpcMarkHit(type);
	}

	[Server]
	public void PickUpGrenade(GrenadeType type)
	{
		GrenadeAssignment g;
		g.type = type;
		g.amount = 1;
		g.id = -1;
		RpcAddGrenades(g);
	}

	public void Shoot(int weaponID, Vector3 hitPosition, Vector3 hitNormal, SurfaceType surfaceType)
	{
		CmdShoot(weaponID, hitPosition, hitNormal, surfaceType, traitsType);
	}

	public void ShootPlayer(int weaponID, int victim, BodyPart bodyPart, Vector3 hitPosition, Vector3 hitNormal)
	{
		CmdShootPlayer(weaponID, victim, bodyPart, hitPosition, hitNormal, traitsType);
	}

	public void ShootDamageable(int weaponID, GameObject other, Vector3 hitPosition, Vector3 hitNormal, SurfaceType surfaceType)
	{
		CmdShootDamageable(weaponID, other, hitPosition, hitNormal, surfaceType, traitsType);
	}

	public void ShootRocket(int weaponID, Vector3 position, Vector3 direction)
	{
		CmdShootRocket(weaponID, position, direction, traitsType);
	}

	public void ShootShotgun(int weaponID, ShotgunShotData data)
	{
		CmdShootShotgun(weaponID, data, traitsType);
	}

	public void Throw(GrenadeType type, Vector3 position, Vector3 forward, float forceRatio)
	{
		CmdThrow(type, position, forward, forceRatio, traitsType);
	}

	public void Melee(int victim, Vector3 position, Vector3 normal, BodyPart bodyPart)
	{
		CmdMelee(victim, position, normal, bodyPart, traitsType);
	}

	public void AddTraitModifier(PlayerTraitModifiersType type)
	{
		if (!traitModifierTypes.Contains(type))
		{
			traitModifierTypes.Add(type);
			UpdateTraitModifiers();
		}
	}

	public void RemoveTraitModifier(PlayerTraitModifiersType type)
	{
		if (traitModifierTypes.Contains(type))
		{
			traitModifierTypes.Remove(type);
			UpdateTraitModifiers();
		}
	}

	/**********************************************************/
	// Commands

	[Command]
	private void CmdRequestStartingWeapons()
	{
		WeaponAssignment weapons;
		WeaponManager.RequestStartingWeapons(networkID, out weapons);

		RpcSetStartingWeapons(weapons);

		List<GrenadeAssignment> grenades;
		GrenadeManager.RequestStartingGrenades(networkID, out grenades);
		foreach (GrenadeAssignment g in grenades)
		{
			RpcAddGrenades(g);
		}
	}

	[Command]
	private void CmdShoot(int weaponID, Vector3 hitPosition, Vector3 hitNormal, SurfaceType surfaceType, PlayerTraitsType traits)
	{
		WeaponManager.Shoot(weaponID, networkID, traits);
		RpcShoot(-1, hitPosition, hitNormal, surfaceType);
	}

	[Command]
	private void CmdShootPlayer(int weaponID, int victim, BodyPart bodyPart, Vector3 hitPosition, Vector3 hitNormal, PlayerTraitsType traits)
	{
		WeaponManager.ShootPlayer(weaponID, networkID, victim, bodyPart, traits);
		RpcShoot(victim, hitPosition, hitNormal, SurfaceType.None);
	}

	[Command]
	private void CmdShootDamageable(int weaponID, GameObject other, Vector3 hitPosition, Vector3 hitNormal, SurfaceType surfaceType, PlayerTraitsType traits)
	{
		WeaponManager.ShootDamageable(weaponID, networkID, other, traits);
		RpcShootDamageable(other, hitPosition, hitNormal, surfaceType);
	}

	[Command]
	private void CmdShootRocket(int weaponID, Vector3 position, Vector3 direction, PlayerTraitsType traits)
	{
		WeaponManager.ShootRocket(weaponID, networkID, position, direction, traits);
		RpcShootRocketLauncher();
	}

	[Command]
	private void CmdShootShotgun(int weaponID, ShotgunShotData data, PlayerTraitsType traits)
	{
		WeaponManager.ShootShotgun(weaponID, networkID, data, traits);
		RpcShootShotgun(data);
	}

	[Command]
	private void CmdThrow(GrenadeType type, Vector3 position, Vector3 forward, float forceRatio, PlayerTraitsType traits)
	{
		GrenadeManager.ThrowGrenade(this, type, position, forward, forceRatio);
	}

	[Command]
	private void CmdMelee(int victim, Vector3 position, Vector3 normal, BodyPart bodyPart, PlayerTraitsType traits)
	{
		DealDamage(PlayerManager.GetPlayer(victim), PartyManager.GameSettings.GetPlayerTraits(traits).Melee.Damage, transform.position, BodyPart.None, DamageType.Melee);
		RpcMelee(victim, position, normal, bodyPart);
	}

	[Command]
	public void CmdSwapWeapon(WeaponType type)
	{
		RpcSwapWeapon(type);
	}

	[Command]
	public void CmdPickUpWeapon(int weaponID, int swappingWeaponID, int ammoInClip)
	{
		WeaponManager.PickUpWeapon(this, weaponID, swappingWeaponID, ammoInClip);
	}

	[Command]
	public void CmdPickUpGrenade(int grenadeID)
	{
		GrenadeManager.PickUpGrenade(this, grenadeID);
	}

	/**********************************************************/
	// Client RPCs

	[ClientRpc]
	private void RpcSetStartingWeapons(WeaponAssignment weapons)
	{
		SetStartingWeapons(weapons);
		weaponCarrier.PrimaryWeapon.Visible = true;

		if (hasAuthority)
		{
			grenadeCarrier.Reset();
			receivedStartingWeapons = true;
		}
	}

	[ClientRpc]
	private void RpcShoot(int victim, Vector3 hitPosition, Vector3 hitNormal, SurfaceType surfaceType)
	{
		if (!hasAuthority && weaponCarrier.PrimaryWeapon)
		{
			weaponCarrier.PrimaryWeapon.ThirdPersonShoot(PlayerManager.GetPlayer(victim), null, hitPosition, hitNormal, surfaceType);
		}
	}

	[ClientRpc]
	private void RpcShootDamageable(GameObject victim, Vector3 hitPosition, Vector3 hitNormal, SurfaceType surfaceType)
	{
		if (!hasAuthority && weaponCarrier.PrimaryWeapon)
		{
			weaponCarrier.PrimaryWeapon.ThirdPersonShoot(null, victim, hitPosition, hitNormal, surfaceType);
		}
	}

	[ClientRpc]
	private void RpcShootRocketLauncher()
	{
		if (!hasAuthority && weaponCarrier.PrimaryWeapon)
		{
			weaponCarrier.PrimaryWeapon.ThirdPersonShoot(null, null, Vector3.zero, Vector3.zero, SurfaceType.None, true);
		}
	}

	[ClientRpc]
	private void RpcShootShotgun(ShotgunShotData data)
	{
		if (!hasAuthority && weaponCarrier.PrimaryWeapon)
		{
			for (int i = 0; i < data.x.Length; i++)
			{
				weaponCarrier.PrimaryWeapon.ThirdPersonShoot(PlayerManager.GetPlayer(data.victim[i]), NetworkServer.FindLocalObject(new NetworkInstanceId(data.damageable[i])), new Vector3(data.x[i], data.y[i], data.z[i]), Vector3.up, SurfaceType.Concrete, data.victim[i] == -2);
			}
		}
	}

	[ClientRpc]
	private void RpcTakeDamage(float damage, int shooter, Vector3 position, BodyPart bodyPart, DamageType type)
	{
		if (health.enabled)
		{
			health.TakeDamage(damage, shooter, position, bodyPart, type);
		}
	}

	[ClientRpc]
	private void RpcDie(Vector3 respawnPos, Quaternion respawnRot, WeaponAssignment weapons, KillData killData, int[] medals)
	{
		hud.ScorePopUp.OnKill(killData);
		hud.KillFeed.OnKill(killData, medals);

		CreateRagdoll(killData);

		SetStartingWeapons(weapons);
		ToggleAliveDead(false);

		if (killData.shooter == PlayerManager.LocalPlayerID)
		{
			hud.MedalPopUp.Trigger(medals);
		}

		for (int i = 0; i < killData.assisters.Length; i++)
		{
			if (killData.assisters[i] == PlayerManager.LocalPlayerID)
			{
				hud.ScorePopUp.Trigger("+" + Mathf.CeilToInt(killData.assistDamages[i]) + " Assist", Color.white);
			}
		}

		rig.position = respawnPos;
		rig.velocity = Vector3.zero;

		if (hasAuthority)
		{
			transform.position = respawnPos;
			rig.position = respawnPos;
			rig.velocity = Vector3.zero;
			GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();
			cam.SetRotation(Quaternion.Euler(0.0f, respawnRot.eulerAngles.y, 0.0f), Quaternion.identity);

			grenadeCarrier.Reset();
			powerUpCarrier.OnDeath();
			netController.OnDeath();

			for (int i = (int)AbilityType.BigHead; i < (int)AbilityType.NumTypes; i++)
			{
				hud.AbilityDisplay.RemoveAbilityIcon((AbilityType)i);
			}
		}

		mod.OnDeath();
	}

	[ClientRpc]
	private void RpcAddAmmo(int weaponID, int ammo)
	{
		if (hasAuthority)
		{
			weaponCarrier.AddAmmo(weaponID, ammo);
		}
	}

	[ClientRpc]
	private void RpcOnRespawn()
	{
		ToggleAliveDead(true);
		health.ResetHealth();

		weaponCarrier.OnRespawn();
		mod.OnRespawn();

		if (hasAuthority)
		{
			netController.Reset();
		}
	}

	[ClientRpc]
	private void RpcSwapWeapon(WeaponType type)
	{
		if (!hasAuthority)
		{
			weaponCarrier.SetThirdPersonWeapon(type);
			weaponCarrier.PrimaryWeapon.Visible = true;
		}
	}

	[ClientRpc]
	private void RpcPickUpWeapon(WeaponType type, int weaponID, int swappingWeaponID, int ammo, int ammoInClip)
	{
		if (hasAuthority)
		{
			weaponCarrier.OnPickUpWeapon(type, weaponID, swappingWeaponID, ammo, ammoInClip);
		}
		else
		{
			weaponCarrier.SetThirdPersonWeapon(type);
			weaponCarrier.PrimaryWeapon.Visible = true;
		}
	}

	[ClientRpc]
	private void RpcMarkHit(HitMarkerType type)
	{
		if (hasAuthority)
		{
			hud.HitMarker.Trigger(type);
		}
	}

	[ClientRpc]
	private void RpcAddGrenades(GrenadeAssignment grenades)
	{
		if (hasAuthority)
		{
			grenadeCarrier.AddGrenades(grenades.type, grenades.amount);
		}
	}

	[ClientRpc]
	public void RpcMelee(int victim, Vector3 position, Vector3 normal, BodyPart bodyPart)
	{
		melee.CreateHitEffects(position, normal, bodyPart);
	}

	[ClientRpc]
	public void RpcAwardMedal(MedalType type)
	{
		if (hasAuthority)
		{
			hud.MedalPopUp.Trigger(type);
		}
	}

	[ClientRpc]
	public void RpcRogiBallTeleport(Vector3 pos)
	{
		if (hasAuthority)
		{
			netController.RogiBallTeleport(pos);
		}
	}

	/**********************************************************/
	// Helper Functions

	private void ToggleAliveDead(bool alive)
	{
		controller.enabled = alive;
		rig.isKinematic = !alive;
		netController.enabled = alive;
		health.enabled = alive;
		hud.SetPlayerNameTagActive(networkID, alive);
		weaponCarrier.enabled = alive;
		grenadeCarrier.enabled = alive;

		if (hasAuthority)
		{
			cam.Enable(alive);
			respawnCam.Enable(!alive, cam);
			mod.FirstPersonHands.Visible = alive;
			mod.FirstPersonLegs.Visible = alive;

			hud.AmmoDisplay.Visible = alive;
			hud.GrenadeSelector.Visible = alive;
			hud.Crosshairs.Alpha = alive ? 1.0f : 0.0f;
			hud.SetAllPlayerNameTagsActive(alive);
			hud.SetSniperScopeVisible(false);
			hud.Radar.SetVisible(alive);
			hud.GrenadeSelector.Visible = alive;
			hud.HealthBar.Visible = alive;
			hud.AbilityDisplay.Visible = alive;

			if (alive)
			{
				hud.RespawnTimer.Deactivate();
				hud.HealthBar.Reset();
				grenadeCarrier.OnRespawn();
			}
			else
			{
				hud.RespawnTimer.Activate(traits.Respawn.Time);
				hud.DamageIndicator.Clear();
				hud.WeaponPickUp.Disable();
				hud.PowerUpSpinner.Hide();
			}
		}
		else
		{
			mod.ThirdPersonModel.Visible = alive;
			if (alive)
			{
				hud.SetPlayerNameTagHealth(ID, 100.0f);
			}
		}

		foreach (BodyPartCollider bp in GetComponentsInChildren<BodyPartCollider>())
		{
			bp.GetComponent<Collider>().enabled = alive;
		}
	}

	private void SetStartingWeapons(WeaponAssignment weapons)
	{
		if (hasAuthority)
		{
			weaponCarrier.SetPrimaryWeapon(weapons.primaryType, weapons.primaryID);
			weaponCarrier.SetSecondaryWeapon(weapons.secondaryType, weapons.secondaryID);
		}
		else
		{
			weaponCarrier.SetThirdPersonWeapon(weapons.primaryType);
		}
	}

	private void CreateRagdoll(KillData killData)
	{
		GameObject obj = Instantiate(ragdollPrefab, mod.ThirdPersonModel.transform.position, transform.rotation);
		obj.GetComponentInChildren<SkinnedMeshRenderer>().material = mod.Material;
		PlayerRagdoll rag = obj.GetComponent<PlayerRagdoll>();

		foreach (Transform t in obj.GetComponentsInChildren<Transform>())
		{
			GameObject other = Utility.FindChild(mod.ThirdPersonModel.gameObject, t.name);
			if (other)
			{
				t.localRotation = other.transform.localRotation;
				t.localScale = other.transform.localScale;
			}
		}

		PlayerCustomizationOptions options = mgr.GetPlayerData(ID).CustomizationOptions;
		Transform headTransform = rag.transform.Find(ThirdPersonModel.GetBodyPartTransformPath(BodyPart.Head));
		rag.ApplyForce(killData.force);
		if (health.SeparatesHead(killData.damageType))
		{
			headTransform = rag.SeparateHead(killData.force);
		}

		if (options.Mask > -1)
		{
			Instantiate(PlayerCustomizer.MaskPrefabs[options.Mask], headTransform);
		}
	}

	private void UpdateTraitModifiers()
	{
		traits.Clone(PartyManager.GameSettings.GetPlayerTraits(traitsType));

		foreach (PlayerTraitModifiersType type in traitModifierTypes)
		{
			traits.Multiply(PartyManager.GameSettings.GetPlayerTraitModifiers(type));
		}

		netController.Traits = traits.Movement;
		melee.Traits = traits.Melee;
		weaponCarrier.Traits = traits.Weapons;
		health.Traits = traits.Health;
		respawn.Traits = traits.Respawn;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Initialized
	{
		get
		{
			return initialized && awoken;
		}
	}

	public int ID
	{
		get
		{
			return networkID;
		}
		set
		{
			networkID = value;
		}
	}

	public string Name
	{
		get
		{
			return userName;
		}
		set
		{
			userName = value;
		}
	}

	public int Team
	{
		get
		{
			return team;
		}
		set
		{
			team = value;
		}
	}

	public PlayerTraitsType Traits
	{
		get
		{
			return traitsType;
		}
		set
		{
			traitsType = value;
			UpdateTraitModifiers();
		}
	}

	public MultiplayerManager MultiplayerManager
	{
		get
		{
			if (!mgr)
			{
				mgr = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
			}
			return mgr;
		}
		set
		{
			mgr = value;
		}
	}

	public MultiplayerMap Map
	{
		get
		{
			return map;
		}
	}

	public PlayerManager PlayerManager
	{
		get
		{
			if (!playerManager)
			{
				playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
			}
			return playerManager;
		}
		set
		{
			playerManager = value;
		}
	}

	public WeaponManager WeaponManager
	{
		get
		{
			if (!weapons)
			{
				weapons = GameObject.Find("WeaponManager").GetComponent<WeaponManager>();
			}
			return weapons;
		}
		set
		{
			weapons = value;
		}
	}

	public GrenadeManager GrenadeManager
	{
		get
		{
			if (!grenades)
			{
				grenades = GameObject.Find("GrenadeManager").GetComponent<GrenadeManager>();
			}
			return grenades;
		}
		set
		{
			grenades = value;
		}
	}

	public PowerUpManager PowerUpManager
	{
		get
		{
			if (!powerUps)
			{
				powerUps = GameObject.Find("PowerUpManager").GetComponent<PowerUpManager>();
			}
			return powerUps;
		}
		set
		{
			powerUps = value;
		}
	}

	public Respawner Respawner
	{
		get
		{
			return respawn;
		}
	}

	public PartyManager Party
	{
		get
		{
			if (!party)
			{
				party = MultiplayerManager.Party;
			}
			return party;
		}
	}

	public CameraManager Cam
	{
		get
		{
			return cam;
		}
	}

	public NetworkPlayerModel Model
	{
		get
		{
			return mod;
		}
	}

	public NetworkPowerUpCarrier PowerUpCarrier
	{
		get
		{
			return powerUpCarrier;
		}
	}

	public Vector3 Center
	{
		get
		{
			return transform.position + Vector3.up;
		}
	}

	public NetworkWeaponCarrier WeaponCarrier
	{
		get
		{
			return weaponCarrier;
		}
	}

	public NetworkCharacterController CharacterController
	{
		get
		{
			return netController;
		}
	}
}