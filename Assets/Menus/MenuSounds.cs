using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSounds : MonoBehaviour
{
	[SerializeField]
	private AudioClip highlightSound;
	[SerializeField]
	private AudioClip clickSound;

	private static MenuSounds instance;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		if (FindObjectsOfType<MenuSounds>().Length > 1)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			aud = GetComponent<AudioSource>();
			DontDestroyOnLoad(gameObject);
		}
	}

	/**********************************************************/
	// Interface

	public static void PlayHighlightSound()
	{
		instance.aud.PlayOneShot(instance.highlightSound);
	}

	public static void PlayClickSound()
	{
		instance.aud.PlayOneShot(instance.clickSound);
	}
}
