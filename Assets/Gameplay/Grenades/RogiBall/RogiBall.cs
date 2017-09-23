using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RogiBall : Grenade
{
	[SerializeField]
	private Material activatedMaterial;

	[SyncVar(hook = "OnActivatedChanged")]
	private bool activated;

	private Vector3 normal;

	/**********************************************************/
	// MonoBehaviour Interface

	public override void Awake()
	{
		base.Awake();

		fuseTime = float.MaxValue;
	}

	public void OnCollisionEnter(Collision col)
	{
		if (isServer)
		{
			normal = col.contacts[0].normal;
			activated = true;
			fuseTime = 5.0f;
		}
	}

	public void OnCollisionStay(Collision col)
	{
		if (isServer)
		{
			normal = col.contacts[0].normal;
		}
	}

	public void FixedUpdate()
	{
		if (isServer && activated)
		{
			Vector3 pos = transform.position + normal * 0.6f;
			//if (Physics.OverlapCapsule(pos + Vector3.down, pos + Vector3.up, 0.55f).Length == 0)
			{
				Detonate();

				NetworkPlayer player = PlayerManager.GetPlayer(Thrower);
				player.RpcRogiBallTeleport(pos + Vector3.down);
			}
		}
	}

	/**********************************************************/
	// Callbacks

	private void OnActivatedChanged(bool value)
	{
		if (value && !activated)
		{
			GetComponentInChildren<MeshRenderer>().material = activatedMaterial;
		}

		activated = value;
	}
}