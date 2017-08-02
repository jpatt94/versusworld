using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonLegs : MonoBehaviour
{
	private int previousJumpSound;

	private OfflinePlayerModel mgr;
	private Animator ani;
	private SkinnedMeshRenderer mesh;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		mgr = GetComponentInParent<OfflinePlayerModel>();
		ani = GetComponent<Animator>();
		mesh = GetComponentInChildren<SkinnedMeshRenderer>();
		aud = GetComponent<AudioSource>();
	}

	/**********************************************************/
	// Interface

	public void OnFootstepAnimation()
	{
		// Handled in ThirdPersonModel.cs
	}

	public void Jump()
	{
		int jumpSound = Random.Range(0, mgr.JumpSounds.Length);
		if (jumpSound == previousJumpSound)
		{
			jumpSound++;
			if (jumpSound >= mgr.JumpSounds.Length)
			{
				jumpSound = 0;
			}
		}
		previousJumpSound = jumpSound;

		aud.PlayOneShot(mgr.JumpSounds[jumpSound]);
	}

	/**********************************************************/
	// Accessors/Mutators

	public Material Material
	{
		set
		{
			mesh.material = value;
		}
	}

	public Animator Animator
	{
		get
		{
			return ani;
		}
	}

	public SkinnedMeshRenderer Mesh
	{
		get
		{
			return mesh;
		}
	}

	public bool Visible
	{
		get
		{
			return mesh.enabled;
		}
		set
		{
			mesh.enabled = value;
		}
	}

}