using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerNameTag : MonoBehaviour
{
	[SerializeField]
	private float flashDuration;

	private bool visible;
	private bool active;
	private float startingHealthBarAlpha;
	private float startingHealthBarBackgroundAlpha;
	private float startingNameOutlineAlpha;
	private float healthBarWidth;
	private float flashTime;
	private float prevHealth;

	private Text text;
	private Image healthBar;
	private Image healthBarFlash;
	private Image healthBarBackground;
	private Outline nameOutline;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		text = transform.Find("NameText").GetComponent<Text>();
		healthBar = transform.Find("HealthBar").GetComponent<Image>();
		healthBarFlash = transform.Find("HealthBarFlash").GetComponent<Image>();
		healthBarBackground = transform.Find("HealthBarBackground").GetComponent<Image>();
		nameOutline = text.GetComponent<Outline>();

		startingHealthBarAlpha = healthBar.color.a;
		startingHealthBarBackgroundAlpha = healthBarBackground.color.a;
		startingNameOutlineAlpha = nameOutline.effectColor.a;
		healthBarWidth = healthBar.rectTransform.sizeDelta.x;
	}

	public void Start()
	{
		visible = false;
		active = true;
	}

	public void Update()
	{
		if (transform.position.z < 0.0f)
		{
			text.enabled = false;
			healthBar.enabled = false;
			healthBarFlash.enabled = false;
			healthBarBackground.enabled = false;
		}

		float alpha = text.color.a;

		if (visible)
		{
			alpha = Mathf.Lerp(alpha, 1.0f, Time.deltaTime * 8.0f);
		}
		else
		{
			alpha = Mathf.Lerp(alpha, 0.0f, Time.deltaTime * 12.0f);
			if (alpha < 0.01f)
			{
				alpha = 0.0f;
				text.enabled = false;
			}
		}

		Utility.SetAlpha(text, alpha);
		Utility.SetAlpha(healthBar, alpha * startingHealthBarAlpha);
		Utility.SetAlpha(healthBarBackground, alpha * startingHealthBarBackgroundAlpha);

		flashTime -= Time.deltaTime;
		Utility.SetAlpha(healthBarFlash, alpha * startingHealthBarAlpha * (flashTime / flashDuration));
	}

	/**********************************************************/
	// Interface

	public void SetHealth(float health)
	{
		Vector2 size = healthBar.rectTransform.sizeDelta;
		size.x = healthBarWidth * Mathf.Clamp01((health / 100.0f));
		healthBar.rectTransform.sizeDelta = size;

		if (health < prevHealth)
		{
			Vector2 offset = healthBarFlash.rectTransform.offsetMax;
			offset.x = healthBarWidth * (prevHealth / 100.0f);
			healthBarFlash.rectTransform.offsetMax = offset;

			offset = healthBarFlash.rectTransform.offsetMin;
			offset.x = healthBarWidth * (health / 100.0f);
			healthBarFlash.rectTransform.offsetMin = offset;

			flashTime = flashDuration;
		}

		prevHealth = health;
	}

	public void SetDistanceRatio(float distanceRatio)
	{
		nameOutline.effectColor = new Color(0.0f, 0.0f, 0.0f, 1.0f - distanceRatio);
	}

	/**********************************************************/
	// Accessors/Mutators

	public void SetVisible(bool visible)
	{
		this.visible = visible;

		if (visible)
		{
			text.enabled = true;
			healthBar.enabled = true;
			healthBarFlash.enabled = true;
			healthBarBackground.enabled = true;
		}
	}

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
		}
	}

	public string Name
	{
		get
		{
			return text.name;
		}
		set
		{
			text.text = value;
		}
	}

	public bool Friendly
	{
		set
		{
			if (value)
			{
				text.color = Color.white;
				healthBar.color = new Color(0.1f, 0.9f, 0.1f, healthBar.color.a);
			}
		}
	}
}
