using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : MonoBehaviour
{
	protected bool active;

	protected MenuManager mgr;
	protected MultiplayerManager multiplayer;
	protected CanvasRenderer[] canvasRenderers;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		active = false;

		mgr = GetComponentInParent<MenuManager>();
		multiplayer = FindObjectOfType<MultiplayerManager>();
		canvasRenderers = GetComponentsInChildren<CanvasRenderer>();
	}

	public virtual void Start()
	{
		Visible = false;
	}

	public virtual void Update()
	{

	}

	public virtual void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public virtual MenuType GetMenuType()
	{
		return MenuType.None;
	}

	public virtual void StateBegin()
	{
		Visible = true;
	}

	public virtual void StateUpdate()
	{

	}

	public virtual void StateEnd()
	{
		Visible = false;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			foreach (CanvasRenderer r in canvasRenderers)
			{
				r.gameObject.SetActive(value);
			}
		}
	}
}

public enum MenuType
{
	Main,
	Customize,
	Settings,
	LobbySettings,
	GameList,
	Lobby,
	NumTypes,
	None,
}