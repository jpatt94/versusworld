using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class KillFeedEntry : MonoBehaviour
{
	[SerializeField]
	private float lerpSpeed;
	[SerializeField]
	private float fadeOutTime;
	[SerializeField]
	private float extraWidth;
	[SerializeField]
	private float popTime;
	[SerializeField]
	private float popScale;
	[SerializeField]
	private GameObject textPrefab;
	[SerializeField]
	private GameObject medalPrefab;
	[SerializeField]
	private float iconSeparation;

	private float timeLeft;
	private float desiredY;
	private float startAlpha;
	private float alphaOverride;
	private float popTimeLeft;

	private RectTransform rect;
	private Image image;
	private Image pop;
	private Text text;
	private Text victimText;
	private Image killTypeImage;
	private List<Image> medalImages;
	private KillFeed mgr;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		rect = GetComponent<RectTransform>();
		image = GetComponent<Image>();
		text = GetComponentInChildren<Text>();
		pop = transform.Find("Pop").GetComponent<Image>();
	}

	public void Start()
	{
		startAlpha = image.color.a;
		alphaOverride = 1.0f;
		popTimeLeft = popTime;
	}

	public void Update()
	{
		timeLeft -= Time.deltaTime;

		float alpha = 1.0f;
		if (timeLeft < fadeOutTime)
		{
			alpha = timeLeft / fadeOutTime;
		}
		alpha = Mathf.Min(alpha, alphaOverride);

		Utility.SetAlpha(image, startAlpha * alpha);
		Utility.SetAlpha(text, alpha);
		if (victimText)
		{
			Utility.SetAlpha(victimText, alpha);
		}
		if (killTypeImage)
		{
			Utility.SetAlpha(killTypeImage, alpha);
		}
		if (medalImages != null)
		{
			foreach (Image i in medalImages)
			{
				Utility.SetAlpha(i, alpha);
			}
		}

		if (!Mathf.Approximately(rect.position.y, desiredY))
		{
			Vector3 newPos = rect.localPosition;
			newPos.y = Mathf.Lerp(newPos.y, desiredY, Time.deltaTime * lerpSpeed);
			rect.localPosition = newPos;
		}

		popTimeLeft -= Time.deltaTime;
		float popAlpha = popTimeLeft / popTime;
		Utility.SetAlpha(pop, popAlpha);
		Vector3 newScale = pop.rectTransform.localScale;
		newScale.x = 1.0f + popScale - popAlpha * popScale;
		newScale.y = popAlpha;
		pop.rectTransform.localScale = newScale;
	}

	/**********************************************************/
	// Interface

	public void SetMessage(string str, Color color)
	{
		text.text = str;
		text.color = color;
		Vector2 size = image.rectTransform.sizeDelta;
		size.x = Utility.CalculateStringWidth(text, str) + text.transform.localPosition.x * 2.0f;
		image.rectTransform.sizeDelta = size;
		pop.rectTransform.sizeDelta = size;
	}

	public void ShowKill(string killerName, Color killerColor, string victimName, Color victimColor, DamageType killType, int[] medals)
	{
		text.text = killerName;
		text.color = killerColor;
		float totalWidth = text.rectTransform.localPosition.x + Utility.CalculateStringWidth(text, killerName + " ");

		GameObject obj = Instantiate(mgr.GetKillTypeImage((int)killType));
		obj.transform.SetParent(transform, false);
		obj.transform.localPosition = Vector3.right * totalWidth;
		totalWidth += obj.GetComponent<RectTransform>().sizeDelta.x;
		killTypeImage = obj.GetComponent<Image>();

		if (medals != null && medals.Length > 0)
		{
			medalImages = new List<Image>();
			foreach (int m in medals)
			{
				obj = Instantiate(medalPrefab);
				obj.transform.SetParent(transform, false);
				totalWidth += iconSeparation;
				obj.transform.localPosition = Vector3.right * totalWidth;
				totalWidth += obj.GetComponent<RectTransform>().sizeDelta.x;

				Image i = obj.GetComponent<Image>();
				i.sprite = mgr.GetMedalImage((MedalType)m);
				medalImages.Add(i);
			}
		}

		obj = Instantiate(textPrefab);
		obj.transform.SetParent(transform, false);
		obj.transform.localPosition = Vector3.right * totalWidth;
		victimText = obj.GetComponent<Text>();
		victimText.text = " " + victimName;
		victimText.color = victimColor;
		totalWidth += Utility.CalculateStringWidth(victimText, " " + victimName);

		Vector2 size = image.rectTransform.sizeDelta;
		size.x = totalWidth + text.rectTransform.localPosition.x;
		image.rectTransform.sizeDelta = size;
		pop.rectTransform.sizeDelta = size;
	}

	/**********************************************************/
	// Accessors/Mutators

	public KillFeed Manager
	{
		set
		{
			mgr = value;
		}
	}

	public float DesiredY
	{
		get
		{
			return desiredY;
		}
		set
		{
			desiredY = value;
		}
	}

	public float TimeLeft
	{
		get
		{
			return timeLeft;
		}
		set
		{
			timeLeft = value;
		}
	}

	public float AlphaOverride
	{
		set
		{
			alphaOverride = value;
		}
	}

	public RectTransform RectTransform
	{
		get
		{
			return rect;
		}
	}
}
