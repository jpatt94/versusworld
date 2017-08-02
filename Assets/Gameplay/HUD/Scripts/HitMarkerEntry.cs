using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HitMarkerEntry : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve positionCurve;
	[SerializeField]
	private AnimationCurve alphaCurve;
	[SerializeField]
	private AnimationCurve headShotPositionCurve;
	[SerializeField]
	private Color headShotColor;
	[SerializeField]
	private Color suppressSniperColor;
	[SerializeField]
	private AnimationCurve suppressSniperAlphaCurve;
	[SerializeField]
	private AnimationCurve suppressSniperPositionCurve;
	[SerializeField]
	private Texture supressSniperTexture;

	private float time;
	private bool suppressSniper;

	private RawImage topLeft;
	private RawImage topRight;
	private RawImage bottomRight;
	private RawImage bottomLeft;
	private RawImage headShot;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		topLeft = transform.Find("TopLeft").GetComponent<RawImage>();
		topRight = transform.Find("TopRight").GetComponent<RawImage>();
		bottomRight = transform.Find("BottomRight").GetComponent<RawImage>();
		bottomLeft = transform.Find("BottomLeft").GetComponent<RawImage>();
		headShot = transform.Find("HeadShot").GetComponent<RawImage>();
	}

	public void Start()
	{
		time = 0.0f;
	}

	public void Update()
	{
		time += Time.deltaTime;

		Animate(topLeft, -1, 1);
		Animate(topRight, 1, 1);
		Animate(bottomRight, 1, -1);
		Animate(bottomLeft, -1, -1);

		Utility.SetAlpha(headShot, suppressSniper ? suppressSniperAlphaCurve.Evaluate(time) : alphaCurve.Evaluate(time));
		headShot.transform.localPosition = new Vector2(0.0f, suppressSniper ? suppressSniperPositionCurve.Evaluate(time) : headShotPositionCurve.Evaluate(time));
	}

	/**********************************************************/
	// Interface

	public void SetHeadShot(bool isHeadShot)
	{
		headShot.enabled = isHeadShot;

		if (isHeadShot)
		{
			topLeft.color = headShotColor;
			topRight.color = headShotColor;
			bottomRight.color = headShotColor;
			bottomLeft.color = headShotColor;
		}
	}

	public void SetSuppressSniper()
	{
		topLeft.color = suppressSniperColor;
		topRight.color = suppressSniperColor;
		bottomRight.color = suppressSniperColor;
		bottomLeft.color = suppressSniperColor;

		suppressSniper = true;
		headShot.texture = supressSniperTexture;
	}

	/**********************************************************/
	// Helper Functions

	private void Animate(RawImage image, int dirX, int dirY)
	{
		float alpha = suppressSniper ? suppressSniperAlphaCurve.Evaluate(time) : alphaCurve.Evaluate(time);
		if (alpha <= 0.0f)
		{
			Destroy(gameObject);
		}
		else
		{
			Utility.SetAlpha(image, alpha);
			float pos = suppressSniper ? suppressSniperPositionCurve.Evaluate(time) : positionCurve.Evaluate(time);
			image.transform.localPosition = new Vector2(pos * dirX, pos * dirY);
		}
	}
}
