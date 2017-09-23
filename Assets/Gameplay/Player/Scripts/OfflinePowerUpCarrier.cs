using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OfflinePowerUpCarrier : SafeNetworkBehaviour
{
	protected NetworkPlayer net;
	protected HUD hud;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		net = GetComponent<NetworkPlayer>();
		hud = GameObject.Find("HUD").GetComponent<HUD>();
	}

	public void OnTriggerEnter(Collider other)
	{
		PowerUp powerUp = other.GetComponent<PowerUp>();
		if (powerUp)
		{
			OnPowerUpCollide(powerUp);
		}
	}

	public void OnTriggerStay(Collider other)
	{
		OnTriggerEnter(other);
	}

	/**********************************************************/
	// Child Interface

	protected virtual void OnPowerUpCollide(PowerUp other)
	{
		Destroy(other.gameObject);
		hud.PowerUpSpinner.Spin((PowerUpType)Random.Range(0, (int)PowerUpType.NumTypes), false);
	}
}
