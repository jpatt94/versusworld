using UnityEngine;
using System.Collections;

public class PlayerHUDManager : MonoBehaviour
{
	[SerializeField]
	private float nameTagFacingThreshold;
	[SerializeField]
	private float minNameTagDistance;
	[SerializeField]
	private float maxNameTagDistnace;
	[SerializeField]
	private float minNameTagScale;
	[SerializeField]
	private float maxNameTagScale;
	[SerializeField]
	private float minNameTagYOffset;
	[SerializeField]
	private float maxNameTagYOffset;
	[SerializeField]
	private AnimationCurve nameTagScaleCurve;

	private bool offline;
	private HUD hud;
	private CameraManager cam;
	private NetworkPlayer net;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		offline = GetComponent<NetworkCharacterController>() == null;
		cam = GetComponentInChildren<CameraManager>();
		net = GetComponent<NetworkPlayer>();
		hud = GameObject.Find("HUD").GetComponent<HUD>();
	}

	public void Start()
	{
	}

	public void LateUpdate()
	{
		if (offline || !net.hasAuthority)
		{
			return;
		}

		foreach (NetworkPlayer player in PlayerManager.PlayerList)
		{
			if (player != net)
			{
				if (hud.GetPlayerNameTagActive(player.ID))
				{
					float distanceAlpha = nameTagScaleCurve.Evaluate(Mathf.Clamp01(Mathf.InverseLerp(minNameTagDistance, maxNameTagDistnace, (cam.transform.position - player.transform.position + Vector3.up).magnitude)));

					Vector3 screenPos = cam.GetComponentInChildren<Camera>().WorldToScreenPoint(player.transform.position + Vector3.up * 2.0f);
					screenPos.y += Mathf.Lerp(minNameTagYOffset, maxNameTagYOffset, distanceAlpha);
					hud.SetPlayerNameTagPosition(player.ID, screenPos);
					hud.SetPlayerNameTagVisible(player.ID, CheckPlayerInSight(player));
					hud.SetPlayerNameTagScale(player.ID, Mathf.Lerp(maxNameTagScale, minNameTagScale, distanceAlpha));
					hud.SetPlayerNameTagDistanceRatio(player.ID, distanceAlpha);
				}
				else
				{
					hud.SetPlayerNameTagVisible(player.ID, false);
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public void CreateNameTag()
	{
		hud.CreatePlayerNameTag(net.ID, net.Name, PartyManager.SameTeam(PartyManager.LocalPlayer.NetworkID, net.ID));
	}

	public void RemoveNameTag()
	{
		hud.RemovePlayerNameTag(net.ID);
	}

	/**********************************************************/
	// Helper Functions

	private bool CheckPlayerInSight(NetworkPlayer player)
	{
		if (player.Respawner.enabled)
		{
			return false;
		}

		Vector3 toPlayer = (player.transform.position + Vector3.up) - cam.transform.position;
		Vector3 camForward = cam.transform.forward;

		float dot = Vector3.Dot(toPlayer.normalized, camForward);
		if (PartyManager.SameTeam(net.ID, player.ID) && dot > 0.0f)
		{
			return true;
		}

		if (dot < nameTagFacingThreshold)
		{
			return false;
		}

		RaycastHit[] hits = Physics.RaycastAll(new Ray(cam.transform.position, toPlayer), toPlayer.magnitude);
		foreach (RaycastHit hit in hits)
		{
			if (hit.collider.tag != "Player")
			{
				return false;
			}
		}

		return true;
	}
}
