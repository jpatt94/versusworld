using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Radar : MonoBehaviour
{
	[SerializeField]
	private float range;
	[SerializeField]
	private float imageRadius;
	[SerializeField]
	private float idleTime;
	[SerializeField]
	private GameObject enemyImagePrefab;
	[SerializeField]
	private Color enemyColor;
	[SerializeField]
	private Color teamColor;

	private Dictionary<int, RadarPlayer> radarPlayers;

	private RawImage radarImage;
	private Text rangeText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		radarPlayers = new Dictionary<int, RadarPlayer>();

		radarImage = GetComponent<RawImage>();
		rangeText = transform.Find("RangeText").GetComponent<Text>();
		rangeText.text = Mathf.RoundToInt(range).ToString() + "m";
	}

	public void LateUpdate()
	{
		if (radarImage && radarImage.enabled && PlayerManager.LocalPlayerID > -1)
		{
			foreach (KeyValuePair<int, RadarPlayer> img in radarPlayers)
			{
				img.Value.image.enabled = false;
			}

			foreach (NetworkPlayer player in PlayerManager.PlayerList)
			{
				if (player.ID != PlayerManager.LocalPlayerID)
				{
					if (!radarPlayers.ContainsKey(player.ID))
					{
						CreateEnemyImage(player.ID);
					}
					ProcessPlayer(player);
				}
			}
		}
	}

	/**********************************************************/
	// Interface

	public void SetVisible(bool visible)
	{
		radarImage.enabled = visible;
		foreach (KeyValuePair<int, RadarPlayer> img in radarPlayers)
		{
			img.Value.image.enabled = img.Value.image.enabled && visible;
		}
		rangeText.enabled = visible;
	}

	/**********************************************************/
	// Helper Functions

	private void ProcessPlayer(NetworkPlayer player)
	{
		NetworkPlayer localPlayer = PlayerManager.LocalPlayer;
		RadarPlayer radarPlayer = radarPlayers[player.ID];

		radarPlayer.timeLeft -= Time.deltaTime;

		Vector3 toEnemy = player.transform.position - PlayerManager.LocalPlayer.transform.position;
		toEnemy.y = 0.0f;

		if (((player.Model.Forward != 0.0f || player.Model.Strafe != 0.0f || player.Model.Jumping) && !player.Model.Crouching) || radarPlayer.didShoot)
		{
			radarPlayer.timeLeft = idleTime;
		}

		float distance = toEnemy.magnitude;
		if (radarPlayer.timeLeft > 0.0f && player.GetComponentInChildren<ThirdPersonModel>().Visible && distance <= range)
		{
			radarPlayer.image.enabled = true;
			Utility.SetAlpha(radarPlayer.image, Mathf.Clamp01(radarPlayer.timeLeft / idleTime));

			Quaternion camRot = localPlayer.GetComponentInChildren<CameraManager>().transform.rotation;
			toEnemy = Quaternion.Euler(0.0f, -camRot.eulerAngles.y, 0.0f) * toEnemy;

			Vector3 newPos = radarPlayer.image.rectTransform.localPosition;
			newPos.x = (toEnemy.x / range) * imageRadius;
			newPos.y = (toEnemy.z / range) * imageRadius;
			radarPlayer.image.rectTransform.localPosition = newPos;
		}
		else
		{
			radarPlayer.image.enabled = false;
		}

		radarPlayer.didShoot = false;
	}

	private void CreateEnemyImage(int playerID)
	{
		GameObject obj = Instantiate(enemyImagePrefab);
		obj.transform.SetParent(transform, false);
		obj.GetComponent<RawImage>().color = PartyManager.SameTeam(playerID, PlayerManager.LocalPlayerID) ? teamColor : enemyColor;
		radarPlayers[playerID] = new RadarPlayer();
		radarPlayers[playerID].image = obj.GetComponent<RawImage>();
	}

	public void SetPlayerDidShoot(NetworkPlayer player)
	{
		RadarPlayer radarPlayer = radarPlayers[player.ID];
		radarPlayer.didShoot = true;
	}
}

class RadarPlayer
{
	public RawImage image;
	public float timeLeft;
	public bool didShoot;
}
