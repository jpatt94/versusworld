using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SafeNetworkBehaviour : NetworkBehaviour
{
	protected bool initialized;
	protected bool needsStart;
	protected bool needsClientStart;
	protected bool needsServerStart;
	protected bool needsAuthorityStart;
	protected bool needsLocalPlayerStart;
	protected bool initializedAuthority;
	protected bool ready;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public virtual void Awake()
	{
	}

	public virtual void Start()
	{
		if (initialized)
		{
			DelayedStart();
		}
		else
		{
			needsStart = true;
		}
	}

	public virtual void Update()
	{
		SafeInitUpdate();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		if (initialized)
		{
			DelayedOnStartClient();
		}
		else
		{
			needsClientStart = true;
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		if (initialized)
		{
			DelayedOnStartServer();
		}
		else
		{
			needsServerStart = true;
		}
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();

		if (initialized)
		{
			DelayedOnStartAuthority();
		}
		else
		{
			needsAuthorityStart = true;
		}
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		if (initialized)
		{
			DelayedOnStartLocalPlayer();
		}
		else
		{
			needsLocalPlayerStart = true;
		}
	}

	/**********************************************************/
	// Interface

	protected void SafeInitUpdate()
	{
		if (!initialized && Ready())
		{
			DelayedAwake();

			if (needsStart)
			{
				DelayedStart();
			}
			if (needsClientStart)
			{
				DelayedOnStartClient();
			}
			if (needsServerStart)
			{
				DelayedOnStartServer();
			}
			if (needsAuthorityStart)
			{
				DelayedOnStartAuthority();
			}
			if (needsLocalPlayerStart)
			{
				DelayedOnStartLocalPlayer();
			}

			initialized = true;
		}
	}

	protected virtual bool Ready()
	{
		return true;
	}

	protected virtual void DelayedAwake()
	{
	}

	protected virtual void DelayedStart()
	{
	}

	protected virtual void DelayedOnStartClient()
	{
	}

	protected virtual void DelayedOnStartServer()
	{
	}

	protected virtual void DelayedOnStartAuthority()
	{
	}

	protected virtual void DelayedOnStartLocalPlayer()
	{
	}
}
