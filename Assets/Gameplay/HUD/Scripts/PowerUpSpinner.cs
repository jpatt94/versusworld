using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PowerUpSpinner : MonoBehaviour
{
	[SerializeField]
	private float activeYOffset;
	[SerializeField]
	private float transitionSpeed;
	[SerializeField]
	private AnimationCurve transitionCurve;
	[SerializeField]
	private float spinDuration;
	[SerializeField]
	private int spinSymbols;
	[SerializeField]
	private AnimationCurve spinCurve;
	[SerializeField]
	private Texture[] symbolTextures;
	[SerializeField]
	private float showDuration;
	[SerializeField]
	private float flashDuration;
	[SerializeField]
	private float flashAlpha;
	[SerializeField]
	private AudioClip spinSound;

	private Vector3 idlePosition;
	private PowerUpSpinnerState state;
	private PowerUpSpinnerState stateAfterSpin;
	private float transitionTime;
	private float spinTime;
	private int currentSymbol;
	private float symbolY;
	private float symbolHeight;
	private PowerUpType landingSymbol;
	private float showTime;
	private float flashTime;

	private RawImage topSymbol;
	private RawImage bottomSymbol;
	private Image flashImage;
	private RectTransform rect;
	private Text pressToUseText;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		rect = GetComponent<RectTransform>();
		topSymbol = transform.Find("Symbol1").GetComponent<RawImage>();
		bottomSymbol = transform.Find("Symbol2").GetComponent<RawImage>();
		flashImage = transform.Find("Flash").GetComponent<Image>();
		pressToUseText = transform.Find("PressToUseText").GetComponent<Text>();
		aud = GetComponentInParent<AudioSource>();
	}

	public void Start()
	{
		idlePosition = rect.transform.localPosition;
		state = PowerUpSpinnerState.Hidden;
		transitionTime = 0.0f;
		symbolY = topSymbol.rectTransform.localPosition.y;
		symbolHeight = topSymbol.rectTransform.sizeDelta.y;

		GameObject mgr = GameObject.Find("MultiplayerManager");
		if (mgr)
		{
			spinDuration = PartyManager.GameSettings.PowerUps.SpinnerDuration;
		}
	}

	public void Update()
	{
		transitionTime = Mathf.Clamp01(transitionTime + Time.deltaTime * transitionSpeed * (state != PowerUpSpinnerState.Hidden ? 1.0f : -1.0f));
		rect.transform.localPosition = Vector3.Lerp(idlePosition, idlePosition + Vector3.up * activeYOffset, transitionCurve.Evaluate(transitionTime));

		if (state == PowerUpSpinnerState.Spinning)
		{
			UpdateSpinning();
		}
		else if (state == PowerUpSpinnerState.Showing)
		{
			UpdateShowing();
		}

		if (state == PowerUpSpinnerState.Showing || state == PowerUpSpinnerState.WaitingForUse)
		{
			flashTime -= Time.deltaTime;
			Utility.SetAlpha(flashImage, (flashTime / flashDuration) * flashAlpha);
		}
	}

	/**********************************************************/
	// Interface

	public void Spin(PowerUpType finalType, bool requiresUse)
	{
		if (state == PowerUpSpinnerState.Spinning)
		{
			Debug.LogWarning("Trying to spin PowerUpSpinner when it's already spinning");
			return;
		}

		state = PowerUpSpinnerState.Spinning;
		stateAfterSpin = requiresUse ? PowerUpSpinnerState.WaitingForUse : PowerUpSpinnerState.Showing;
		spinTime = 0.0f;
		currentSymbol = 0;
		landingSymbol = finalType;
		Utility.SetAlpha(flashImage, 0.0f);

		aud.PlayOneShot(spinSound);
	}

	public void Use()
	{
		if (state != PowerUpSpinnerState.WaitingForUse)
		{
			Debug.LogWarning("Calling PowerUpSpinner.Use when not waiting for a use");
			return;
		}

		Hide();
	}

	public void Hide()
	{
		state = PowerUpSpinnerState.Hidden;
		pressToUseText.enabled = false;
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateSpinning()
	{
		spinTime += Time.deltaTime;
		float spinAlpha = spinCurve.Evaluate(spinTime / spinDuration);
		float symbolRatio = 1.0f / spinSymbols;
		float nextSymbol = currentSymbol * symbolRatio;

		if (spinAlpha >= nextSymbol)
		{
			if (currentSymbol < spinSymbols)
			{
				bottomSymbol.texture = topSymbol.texture;

				if (currentSymbol == spinSymbols - 1)
				{
					topSymbol.texture = symbolTextures[(int)landingSymbol];
				}
				else
				{
					topSymbol.texture = symbolTextures[(int)PowerUpManager.GetRandomPowerUpType()];
				}

				currentSymbol++;
			}
		}

		float betweenAlpha = (spinAlpha % symbolRatio) / symbolRatio;

		Rect newUVs = topSymbol.uvRect;
		newUVs.y = betweenAlpha;
		topSymbol.uvRect = newUVs;

		Vector2 newSize = bottomSymbol.rectTransform.sizeDelta;
		newSize.y = symbolHeight * (1.0f - betweenAlpha);
		bottomSymbol.rectTransform.sizeDelta = newSize;

		Vector3 newPos = bottomSymbol.rectTransform.localPosition;
		newPos.y = symbolY - symbolHeight * betweenAlpha;
		bottomSymbol.rectTransform.localPosition = newPos;

		newUVs = bottomSymbol.uvRect;
		newUVs.y = betweenAlpha;
		newUVs.height = 1.0f - betweenAlpha;
		bottomSymbol.uvRect = newUVs;

		if (spinTime >= spinDuration)
		{
			OnSpinLand();
		}
	}

	private void UpdateShowing()
	{
		showTime -= Time.deltaTime;
		if (showTime <= 0.0f)
		{
			state = PowerUpSpinnerState.Hidden;
		}
	}

	private void OnSpinLand()
	{
		state = stateAfterSpin;

		if (state == PowerUpSpinnerState.Showing)
		{
			showTime = showDuration;
		}
		else if (state == PowerUpSpinnerState.WaitingForUse)
		{
			pressToUseText.enabled = true;
		}

		flashTime = flashDuration;

		JP.Event.Trigger("OnPowerUpSpinnerDone");
	}
}

public enum PowerUpSpinnerState
{
	Hidden,
	Spinning,
	Showing,
	WaitingForUse,
}