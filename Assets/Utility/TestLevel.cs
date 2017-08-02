using UnityEngine;
using System.Collections;

public class TestLevel : MonoBehaviour
{
	[SerializeField]
	private GameObject offlinePlayer;
	[SerializeField]
	private GameObject spawnPoint;
	[SerializeField]
	private GameObject gameSettingsPrefab;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		if (GameObject.Find("MultiplayerManager") == null)
		{
			PartyManager.GameSettings = Instantiate(gameSettingsPrefab).GetComponent<GameSettings>();
		}
	}

	public void Start()
	{
		if (GameObject.Find("MultiplayerManager") == null)
		{
			Instantiate(offlinePlayer, spawnPoint.transform.position, spawnPoint.transform.rotation);
			Destroy(GameObject.Find("StartGameCanvas"));
		}

		PlayerInput.Enabled = true;

		ControlSettings.Load();
		VideoSettings.Load();
		AudioSettings.Load();
	}

	public void Update()
	{

	}
}
