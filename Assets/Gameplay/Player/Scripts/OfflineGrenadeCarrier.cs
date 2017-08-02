using UnityEngine;
using System.Collections;

public class OfflineGrenadeCarrier : MonoBehaviour
{
	[SerializeField]
	protected float throwRate;
	[SerializeField]
	protected Vector3 throwPosition;
	[SerializeField]
	protected Vector3 jerk;
	[SerializeField]
	protected float jerkDuration;

	protected int[] grenades;
	protected GrenadeType currentType;
	protected float canThrow;

	protected HUD hud;
	protected OfflinePlayerModel playerModel;
	protected OfflineMelee melee;
	protected CameraManager cam;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		OnAwake();
	}

	public void Start()
	{
		OnStart();
	}

	public void Update()
	{
		OnUpdate();
	}

	/**********************************************************/
	// Interface

	public void AddGrenades(GrenadeType type, int amount)
	{
		grenades[(int)type] += amount;
		hud.GrenadeSelector.SetGrenadeAmount(type, grenades[(int)type]);

		if (currentType == GrenadeType.None)
		{
			SelectNextGrenade();
		}
	}

	public void Reset()
	{
		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			grenades[i] = 0;
			hud.GrenadeSelector.SetGrenadeAmount((GrenadeType)i, 0);
		}
		hud.GrenadeSelector.SelectedType = GrenadeType.None;
	}

	/**********************************************************/
	// Child Interface

	protected virtual void OnAwake()
	{
		grenades = new int[(int)GrenadeType.NumTypes];

		hud = GameObject.Find("HUD").GetComponent<HUD>();
		playerModel = GetComponent<OfflinePlayerModel>();
		melee = GetComponent<OfflineMelee>();
		cam = GetComponentInChildren<CameraManager>();
	}

	protected virtual void OnStart()
	{
		currentType = GrenadeType.None;

		canThrow = 0.0f;
	}

	protected virtual void OnUpdate()
	{
		if (PlayerInput.NextGrenade())
		{
			SelectNextGrenade();
		}
		else if (PlayerInput.PreviousGrenade())
		{
			SelectPreviousGrenade();
		}

		canThrow += Time.deltaTime;

		if (PlayerInput.Throw(ButtonStatus.Pressed) && canThrow >= 0.0f && currentType != GrenadeType.None && grenades[(int)currentType] > 0 && !melee.Active)
		{
			canThrow = -(1.0f / throwRate);
			grenades[(int)currentType]--;
			hud.GrenadeSelector.SetGrenadeAmount(currentType, grenades[(int)currentType]);

			ThrowGrenade();
		}
	}

	protected virtual void ThrowGrenade()
	{
		playerModel.OnThrow();

		if (grenades[(int)currentType] <= 0)
		{
			SelectNextGrenade();
		}

		cam.Jerk(jerk, jerkDuration, true);
		cam.ZShake(jerk.z, jerkDuration);
	}

	/**********************************************************/
	// Helper Functions

	protected void SelectFirstGrenade()
	{
		currentType = GrenadeType.None;

		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			if (grenades[i] > 0)
			{
				currentType = (GrenadeType)i;
				break;
			}
		}

		hud.GrenadeSelector.SelectedType = currentType;
	}

	protected void SelectNextGrenade()
	{
		int prevType = (int)currentType;
		currentType = GrenadeType.None;

		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			int index = (prevType + i + 1) % (int)GrenadeType.NumTypes;
			if (grenades[index] > 0)
			{
				currentType = (GrenadeType)index;
				break;
			}
		}

		hud.GrenadeSelector.SelectedType = currentType;
	}

	protected void SelectPreviousGrenade()
	{
		int prevType = (int)currentType;
		currentType = GrenadeType.None;

		for (int i = 0; i < (int)GrenadeType.NumTypes; i++)
		{
			int index = (prevType - i - 1 + (int)GrenadeType.NumTypes * 2) % (int)GrenadeType.NumTypes;
			if (grenades[index] > 0)
			{
				currentType = (GrenadeType)index;
				break;
			}
		}

		hud.GrenadeSelector.SelectedType = currentType;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool CanThrow
	{
		get
		{
			return canThrow >= 0.0f;
		}
	}
}
