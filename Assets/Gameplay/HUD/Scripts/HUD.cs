using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	[SerializeField]
	private GameObject playerNameTagPrefab;
	[SerializeField]
	private Vector2 waypointMin;
	[SerializeField]
	private Vector2 waypointMax;

	private Camera cam;
	private Canvas staticCanvas;
	private Canvas shakeCanvas;
	private AmmoDisplay ammoDisplay;
	private GrenadeSelector grenadeSelector;
	private Dictionary<int, PlayerNameTag> playerNameTags;
	private RawImage bloodOverlay;
	private ScorePopUp scorePopUp;
	private RespawnTimer respawnTimer;
	private Crosshairs crosshairs;
	private RawImage sniperScope;
	private WeaponPickUp weaponPickUp;
	private Radar radar;
	private KillFeed killFeed;
	private HitMarker hitMarker;
	private MedalPopUp medalPopUp;
	private PowerUpSpinner powerUpSpinner;
	private DamageIndicator damageIndicator;
	private QuickScoreboard quickScoreboard;
	private GameOverPanel gameOverPanel;
	private HealthBar healthBar;
	private Scoreboard scoreboard;
	private AbilityDisplay abilityDisplay;

	private static HUD instance;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		cam = GetComponentInChildren<Camera>();
		staticCanvas = transform.Find("StaticCanvas").GetComponent<Canvas>();
		shakeCanvas = transform.Find("ShakeCanvas").GetComponent<Canvas>();
		ammoDisplay = GetComponentInChildren<AmmoDisplay>();
		grenadeSelector = GetComponentInChildren<GrenadeSelector>();
		playerNameTags = new Dictionary<int, PlayerNameTag>();
		bloodOverlay = GameObject.Find("BloodOverlay").GetComponent<RawImage>();
		scorePopUp = GetComponentInChildren<ScorePopUp>();
		respawnTimer = GetComponentInChildren<RespawnTimer>();
		crosshairs = GetComponentInChildren<Crosshairs>();
		sniperScope = Utility.FindChild(gameObject, "SniperScope").GetComponent<RawImage>();
		weaponPickUp = GetComponentInChildren<WeaponPickUp>();
		radar = GetComponentInChildren<Radar>();
		killFeed = GetComponentInChildren<KillFeed>();
		hitMarker = GetComponentInChildren<HitMarker>();
		medalPopUp = GetComponentInChildren<MedalPopUp>();
		powerUpSpinner = GetComponentInChildren<PowerUpSpinner>();
		damageIndicator = GetComponentInChildren<DamageIndicator>();
		quickScoreboard = GetComponentInChildren<QuickScoreboard>();
		gameOverPanel = GetComponentInChildren<GameOverPanel>();
		healthBar = GetComponentInChildren<HealthBar>();
		scoreboard = GameObject.Find("Scoreboard").GetComponent<Scoreboard>();
		abilityDisplay = GetComponentInChildren<AbilityDisplay>();

		healthBar.Initialize();
		SetHealth(1.0f);

		staticCanvas.enabled = true;
		shakeCanvas.enabled = true;
		bloodOverlay.enabled = true;

		instance = this;
	}

	public void Update()
	{
		if (VisualDebugger.Enabled && Input.GetKey(KeyCode.LeftShift))
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				hitMarker.Trigger(HitMarkerType.Friendly);
			}
			if (Input.GetKeyDown(KeyCode.P))
			{
				scorePopUp.Trigger("+50 Assist", Color.white);
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				//int[] medals = new int[2];
				//medals[0] = (int)MedalType.KillingSpree;
				//medals[1] = (int)MedalType.DoubleKill;
				//killFeed.ShowKill("Justin", Color.yellow, "Ichiban", Color.yellow, DamageType.Melee, medals);

				KillFeed.ShowMessage("jpatt94: Cloudy with a chance of grenades!", Color.white);
			}
			if (Input.GetKeyDown(KeyCode.M))
			{
				medalPopUp.Trigger((MedalType)(Random.Range(0, (int)MedalType.NumMedals)));
			}
			if (Input.GetKeyDown(KeyCode.O))
			{
				medalPopUp.Trigger((MedalType)(Random.Range(0, (int)MedalType.NumMedals)));
				medalPopUp.Trigger((MedalType)(Random.Range(0, (int)MedalType.NumMedals)));
				medalPopUp.Trigger((MedalType)(Random.Range(0, (int)MedalType.NumMedals)));
				medalPopUp.Trigger((MedalType)(Random.Range(0, (int)MedalType.NumMedals)));
				medalPopUp.Trigger((MedalType)(Random.Range(0, (int)MedalType.NumMedals)));
				scorePopUp.Trigger("+100 Kill", Color.white);
				scorePopUp.Trigger("+100 Kill", Color.white);
				scorePopUp.Trigger("+100 Kill", Color.white);
				scorePopUp.Trigger("+100 Kill", Color.white);
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				powerUpSpinner.Spin(PowerUpManager.GetRandomPowerUpType(), false);
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				healthBar.SetHealth(0.0f);
			}
			if (Input.GetKeyDown(KeyCode.Y))
			{
				healthBar.SetHealth(33.0f);
			}
			if (Input.GetKeyDown(KeyCode.U))
			{
				healthBar.SetHealth(66.0f);
			}
			if (Input.GetKeyDown(KeyCode.I))
			{
				healthBar.SetHealth(100.0f);
			}
			for (int i = (int)KeyCode.Alpha1; i < (int)KeyCode.Alpha1 + (int)GrenadeType.NumTypes; i++)
			{
				if (Input.GetKeyDown((KeyCode)i))
				{
					GrenadeType type = (GrenadeType)(i - (int)KeyCode.Alpha1);
					GameObject.FindGameObjectWithTag("Player").GetComponent<OfflineGrenadeCarrier>().AddGrenades(type, 1);
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public void CreatePlayerNameTag(int playerID, string name, bool friendly)
	{
		GameObject nameTag = Instantiate(playerNameTagPrefab);
		nameTag.transform.SetParent(Utility.FindChild(gameObject, "PlayerNameTags").transform, false);
		nameTag.GetComponent<PlayerNameTag>().Name = name;
		nameTag.GetComponent<PlayerNameTag>().Friendly = friendly;
		playerNameTags[playerID] = nameTag.GetComponent<PlayerNameTag>();
	}

	public void RemovePlayerNameTag(int playerID)
	{
		print("Removing name tag for " + playerID);

		Destroy(playerNameTags[playerID].gameObject);
		playerNameTags.Remove(playerID);
	}

	public void SetPlayerNameTagPosition(int playerID, Vector3 position)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return;
		}

		playerNameTags[playerID].transform.position = position;
	}

	public void SetPlayerNameTagVisible(int playerID, bool visible)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return;
		}

		playerNameTags[playerID].SetVisible(visible);
	}

	public void SetPlayerNameTagActive(int playerID, bool active)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return;
		}

		playerNameTags[playerID].Active = active;
	}

	public bool GetPlayerNameTagActive(int playerID)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return false;
		}

		return playerNameTags[playerID].Active;
	}

	public void SetAllPlayerNameTagsActive(bool active)
	{
		foreach (KeyValuePair<int, PlayerNameTag> kv in playerNameTags)
		{
			kv.Value.Active = active;
		}
	}

	public void SetPlayerNameTagHealth(int playerID, float health)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return;
		}

		playerNameTags[playerID].SetHealth(health);
	}

	public void SetPlayerNameTagScale(int playerID, float scale)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return;
		}

		playerNameTags[playerID].transform.localScale = Vector3.one * scale;
	}

	public void SetPlayerNameTagDistanceRatio(int playerID, float ratio)
	{
		if (!playerNameTags.ContainsKey(playerID))
		{
			return;
		}

		playerNameTags[playerID].SetDistanceRatio(ratio);
	}

	public void SetHealth(float health)
	{
		//float alpha = Mathf.Clamp01(1.0f - (health - 0.25f) / 0.75f);
		bloodOverlay.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(1.0f - health));
		healthBar.SetHealth(health * 100.0f);
	}

	public void SetSniperScopeVisible(bool visible)
	{
		sniperScope.enabled = visible;
	}

	/**********************************************************/
	// Accessors

	public static HUD Instance
	{
		get
		{
			return instance;
		}
	}

	public Camera Camera
	{
		get
		{
			return cam;
		}
	}

	public Canvas StaticCanvas
	{
		get
		{
			return staticCanvas;
		}
	}

	public Canvas ShakeCanvas
	{
		get
		{
			return shakeCanvas;
		}
	}

	public AmmoDisplay AmmoDisplay
	{
		get
		{
			return ammoDisplay;
		}
	}

	public GrenadeSelector GrenadeSelector
	{
		get
		{
			return grenadeSelector;
		}
	}

	public ScorePopUp ScorePopUp
	{
		get
		{
			return scorePopUp;
		}
	}

	public RespawnTimer RespawnTimer
	{
		get
		{
			return respawnTimer;
		}
	}

	public Crosshairs Crosshairs
	{
		get
		{
			return crosshairs;
		}
	}

	public WeaponPickUp WeaponPickUp
	{
		get
		{
			return weaponPickUp;
		}
	}

	public Radar Radar
	{
		get
		{
			return radar;
		}
	}

	public KillFeed KillFeed
	{
		get
		{
			return killFeed;
		}
	}

	public HitMarker HitMarker
	{
		get
		{
			return hitMarker;
		}
	}

	public MedalPopUp MedalPopUp
	{
		get
		{
			return medalPopUp;
		}
	}

	public PowerUpSpinner PowerUpSpinner
	{
		get
		{
			return powerUpSpinner;
		}
	}

	public DamageIndicator DamageIndicator
	{
		get
		{
			return damageIndicator;
		}
	}

	public QuickScoreboard QuickScoreboard
	{
		get
		{
			return quickScoreboard;
		}
	}

	public GameOverPanel GameOverPanel
	{
		get
		{
			return gameOverPanel;
		}
	}

	public HealthBar HealthBar
	{
		get
		{
			return healthBar;
		}
	}

	public Scoreboard Scoreboard
	{
		get
		{
			return scoreboard;
		}
	}

	public AbilityDisplay AbilityDisplay
	{
		get
		{
			return abilityDisplay;
		}
	}

	public static Vector2 WaypointMin
	{
		get
		{
			return instance.waypointMin;
		}
	}

	public static Vector2 WaypointMax
	{
		get
		{
			return instance.waypointMax;
		}
	}
}
