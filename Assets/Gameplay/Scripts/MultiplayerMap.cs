using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MultiplayerMap : NetworkBehaviour
{
	[SerializeField]
	private Sprite mapOverview;

	private float width;
	private float length;
	private float killY;

	private SpawnManager spawnMgr;
	private Transform topLeftCorner;
	private Transform bottomRightCorner;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		topLeftCorner = GameObject.Find("MapTopLeftCorner").GetComponent<Transform>();
		bottomRightCorner = GameObject.Find("MapBottomRightCorner").GetComponent<Transform>();

		width = Mathf.Abs(topLeftCorner.position.x - bottomRightCorner.position.x);
		length = Mathf.Abs(topLeftCorner.position.z - bottomRightCorner.position.z);
		killY = GameObject.Find("MapKillY").transform.position.y;

		spawnMgr = GetComponent<SpawnManager>();
	}

	public void Start()
	{
		PlayerInput.Enabled = true;
		Cursor.lockState = CursorLockMode.Locked;

		ControlSettings.Load();
		VideoSettings.Load();
		AudioSettings.Load();
	}

	public void Update()
	{
	}

	/**********************************************************/
	// Interface

	public Vector3 ConvertMapUVToPosition(float u, float v)
	{
		return topLeftCorner.position + (topLeftCorner.right * u * Width - topLeftCorner.forward * v * Length);
	}

	/**********************************************************/
	// Accessors/Mutators

	public float Width
	{
		get
		{
			return width;
		}
	}

	public float Length
	{
		get
		{
			return length;
		}
	}

	public float KillY
	{
		get
		{
			return killY;
		}
	}

	public SpawnManager SpawnManager
	{
		get
		{
			return spawnMgr;
		}
	}

	public Sprite MapOverview
	{
		get
		{
			return mapOverview;
		}
	}
}