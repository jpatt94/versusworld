using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MedalPopUpEntry : MonoBehaviour
{
	[SerializeField]
	private float duration;
	[SerializeField]
	private float lerpSpeed;
	[SerializeField]
	private float flashTime;
	[SerializeField]
	private float flashScale;
	[SerializeField]
	private AnimationCurve alphaCurve;
	[SerializeField]
	private AnimationCurve positionAlphaCurve;

	private float time;
	private float desiredPos;

	private Image image;
	private RawImage flash;
	private Text nameText;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		image = GetComponent<Image>();
		flash = transform.Find("Flash").GetComponent<RawImage>();
		nameText = transform.Find("NameText").GetComponent<Text>();
	}

	public void Start()
	{
		time = 0.0f;
	}

	public void Update()
	{
		time += Time.deltaTime;

		Utility.SetAlpha(image, Mathf.Min(alphaCurve.Evaluate(time), positionAlphaCurve.Evaluate(Mathf.Abs(transform.localPosition.x))));
		Utility.SetAlpha(nameText, image.color.a);

		Vector2 newPos = transform.localPosition;
		newPos.x = Mathf.Lerp(newPos.x, desiredPos, Time.deltaTime * lerpSpeed);
		transform.localPosition = newPos;

		float flashAlpha = time / flashTime;
		Utility.SetAlpha(flash, 1.0f - flashAlpha);
		Vector2 newScale = flash.transform.localScale;
		newScale.x = Mathf.Lerp(1.0f, flashScale, flashAlpha);
		newScale.y = Mathf.Lerp(1.0f, flashScale, flashAlpha);
		flash.transform.localScale = newScale;
	}

	/**********************************************************/
	// Accessors/Mutators

	public string Name
	{
		set
		{
			nameText.text = value;
		}
	}

	public float Duration
	{
		get
		{
			return duration;
		}
	}

	public float CurrentTime
	{
		get
		{
			return time;
		}
	}

	public float DesiredPosition
	{
		get
		{
			return desiredPos;
		}
		set
		{
			desiredPos = value;
		}
	}

	public float Alpha
	{
		get
		{
			return image.color.a;
		}
	}
}
