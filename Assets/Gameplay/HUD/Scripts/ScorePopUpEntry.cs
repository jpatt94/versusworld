using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScorePopUpEntry : MonoBehaviour
{
	public float duration;
	public AnimationCurve scale;
	public AnimationCurve alpha;
	public float chainLerpSpeed;
	public AnimationCurve chainPosCurve;
	public AnimationCurve chainScaleCurve;
	public AnimationCurve chainAlphaCurve;

	private float time;
	private float chainPosition;
	private float desiredChainPosition;

	private Text text;

	void Awake()
	{
		text = GetComponent<Text>();
	}

	void Start()
	{
		time = 0.0f;
	}

	void Update()
	{
		chainPosition = Mathf.Lerp(chainPosition, desiredChainPosition, Time.deltaTime * chainLerpSpeed);

		Vector3 newPos = transform.localPosition;
		newPos.y = chainPosCurve.Evaluate(chainPosition);
		transform.localPosition = newPos;

		float s = scale.Evaluate(time / duration) * chainScaleCurve.Evaluate(chainPosition);
		float a = alpha.Evaluate(time / duration) * chainAlphaCurve.Evaluate(chainPosition);

		transform.localScale = Vector3.one * s;
		Utility.SetAlpha(text, a);

		time += Time.deltaTime;
	}

	/**********************************************************/
	// Accessors/Mutators

	public float CurrentTime
	{
		get
		{
			return time;
		}
	}

	public float ChainPosition
	{
		get
		{
			return desiredChainPosition;
		}
		set
		{
			desiredChainPosition = value;
		}
	}

	public Text Text
	{
		get
		{
			return text;
		}
	}
}
