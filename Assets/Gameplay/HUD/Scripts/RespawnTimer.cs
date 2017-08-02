using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RespawnTimer : MonoBehaviour
{
	[SerializeField]
	private AudioClip deathSound;
	[SerializeField]
	private AudioClip respawnSound;
	[SerializeField]
	private AudioClip tickSound;

	private float time;
	private float nextTickSound;

	private Text text;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	void Awake()
	{
		text = GetComponent<Text>();
		aud = GetComponentInParent<AudioSource>();
	}

	void Update()
	{
		time -= Time.deltaTime;

		if (time <= nextTickSound && nextTickSound >= 0.5f)
		{
			nextTickSound -= 1.0f;
			aud.PlayOneShot(tickSound);
		}

		string seconds = string.Format("{0:0.0}", Mathf.Max(0.0f, time));
		text.text = "Respawing in " + seconds;
	}

	/**********************************************************/
	// Interface

	public void Activate(float respawnTime)
	{
		enabled = true;
		text.enabled = true;

		time = respawnTime;
		nextTickSound = time - 1.0f;

		aud.PlayOneShot(deathSound);
	}

	public void Deactivate()
	{
		enabled = false;
		text.enabled = false;

		aud.PlayOneShot(respawnSound);
	}
}
