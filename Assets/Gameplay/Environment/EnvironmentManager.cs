using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
	[SerializeField]
	private GameObject[] playerHitEffect;
	[SerializeField]
	private GameObject[] hitEffects;
	[SerializeField]
	private AudioClip[] grassHitSounds;
	[SerializeField]
	private AudioClip[] woodHitSounds;
	[SerializeField]
	private AudioClip[] stoneHitSounds;
	[SerializeField]
	private AudioClip[] metalHitSounds;

	private static EnvironmentManager instance;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		instance = this;
	}

	/**********************************************************/
	// Interface

	public static GameObject GetPlayerHitEffect(int size)
	{
		return instance.playerHitEffect[size];
	}

	public static GameObject GetHitEffect(SurfaceType type)
	{
		return instance.hitEffects[(int)type];
	}

	public static SurfaceType ConvertSurfaceNameToType(string name)
	{
		switch (name)
		{
			case "Grass": return SurfaceType.Grass;
			case "Wood": return SurfaceType.Wood;
			case "Concrete": return SurfaceType.Concrete;
			case "Metal": return SurfaceType.Metal;
		}

		return SurfaceType.None;
	}

	public static AudioClip GetRandomHitEffectSound(SurfaceType type)
	{
		switch (type)
		{
			case SurfaceType.Grass: return instance.grassHitSounds[Random.Range(0, instance.grassHitSounds.Length)];
			case SurfaceType.Wood: return instance.woodHitSounds[Random.Range(0, instance.woodHitSounds.Length)];
			case SurfaceType.Concrete: return instance.stoneHitSounds[Random.Range(0, instance.stoneHitSounds.Length)];
			case SurfaceType.Metal: return instance.metalHitSounds[Random.Range(0, instance.metalHitSounds.Length)];
		}

		return instance.metalHitSounds[0];
	}
}

public enum SurfaceType
{
	Grass,
	Wood,
	Concrete,
	Metal,
	NumTypes,
	None,
}