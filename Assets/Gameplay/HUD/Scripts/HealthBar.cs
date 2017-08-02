using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField]
	private float margin;
	[SerializeField]
	private float deathFlashDuration;
	[SerializeField]
	private float fadeOutSpeed;
	[SerializeField]
	private float startingAlpha;
	[SerializeField]
	private AnimationCurve alphaCurve;

	private float prevHealth;
	private float zeroLifeOffset;
	private float zeroDeathOffset;
	private float deathFlashTime;
	private float alpha;

	private Image backgroundImage;
	private Image lifeBar;
	private Image deathBar;
	private Image deathFlashBar;
	private Text healthText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Update()
	{
		if (prevHealth >= 100.0f)
		{
			alpha -= Time.deltaTime * fadeOutSpeed;
		}
		alpha = Mathf.Clamp01(alpha);
		float thisAlpha = alphaCurve.Evaluate(alpha);

		deathFlashTime -= Time.deltaTime;
		Utility.SetAlpha(deathFlashBar, (deathFlashTime / deathFlashDuration) * thisAlpha * startingAlpha);

		Utility.SetAlpha(backgroundImage, thisAlpha * startingAlpha);
		Utility.SetAlpha(lifeBar, thisAlpha * startingAlpha);
		Utility.SetAlpha(deathBar, thisAlpha * startingAlpha);
		Utility.SetAlpha(healthText, thisAlpha);
	}

	/**********************************************************/
	// Interface

	public void Initialize()
	{
		backgroundImage = GetComponent<Image>();
		lifeBar = transform.Find("LifeBar").GetComponent<Image>();
		deathBar = transform.Find("DeathBar").GetComponent<Image>();
		deathFlashBar = transform.Find("DeathFlashBar").GetComponent<Image>();
		healthText = transform.Find("HealthText").GetComponent<Text>();

		zeroLifeOffset = lifeBar.rectTransform.offsetMax.x;
		zeroDeathOffset = deathBar.rectTransform.offsetMin.x;
	}

	public void Reset()
	{
		alpha = 0.0f;
		SetHealth(100.0f);
	}

	public void SetHealth(float health)
	{
		if (health < 100.0f)
		{
			alpha = 1.0f;
		}

		healthText.text = Mathf.Max(1, Mathf.FloorToInt(health)).ToString();

		Vector2 lifeOffset = lifeBar.rectTransform.offsetMax;
		lifeOffset.x = Mathf.Lerp(zeroLifeOffset,- margin, health / 100.0f);
		lifeBar.rectTransform.offsetMax = lifeOffset;

		Vector2 deathOffset = deathBar.rectTransform.offsetMin;
		deathOffset.x = Mathf.Lerp(margin, zeroDeathOffset, health / 100.0f);
		deathBar.rectTransform.offsetMin = deathOffset;

		if (health < prevHealth)
		{
			Vector2 maxOffset = lifeBar.rectTransform.offsetMax;
			maxOffset.x = Mathf.Lerp(zeroLifeOffset, -margin, prevHealth / 100.0f);
			deathFlashBar.rectTransform.offsetMax = maxOffset;

			Vector2 minOffset = deathBar.rectTransform.offsetMin;
			minOffset.x = Mathf.Lerp(margin, zeroDeathOffset, health / 100.0f);
			deathFlashBar.rectTransform.offsetMin = minOffset;

			deathFlashTime = deathFlashDuration;
		}

		prevHealth = health;
	}

	/**********************************************************/
	// Accessors/Mutators
	
	public bool Visible
	{
		get
		{
			return backgroundImage.enabled;
		}
		set
		{
			backgroundImage.enabled = value;
			lifeBar.enabled = value;
			//deathBar.enabled = value;
			deathFlashBar.enabled = value;
			healthText.enabled = value;
		}
	}
}