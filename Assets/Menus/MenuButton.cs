using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class MenuButton : Button
{
	[SerializeField]
	private Color highlightColor;
	[SerializeField]
	private float highlightMaxScale;
	[SerializeField]
	private float highlightMinScale;
	[SerializeField]
	private float scaleRate;

	private string eventName;
	private bool highlighted;

	private Graphic graphic;

	/**********************************************************/
	// MonoBehaviour Interface

	protected override void Awake()
	{
		base.Awake();

		eventName = "On" + gameObject.name + "Click";

		graphic = GetComponent<Text>();
		if (!graphic)
		{
			graphic = GetComponent<Image>();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		highlighted = false;
		graphic.color = colors.normalColor;
		graphic.rectTransform.localScale = Vector3.one;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);

		highlighted = true;
		graphic.color = highlightColor;
		graphic.rectTransform.localScale = Vector3.one * highlightMaxScale;

		MenuSounds.PlayHighlightSound();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);

		highlighted = false;
		graphic.color = colors.normalColor;
	}

	public void Update()
	{
		graphic.color = highlighted ? highlightColor : colors.normalColor;
		graphic.rectTransform.localScale = Vector3.one * (Mathf.Max(graphic.rectTransform.localScale.x - scaleRate * Time.deltaTime, highlighted ? highlightMinScale : 1.0f));
	}

	/**********************************************************/
	// Interface

	public void OnClick()
	{
		JP.Event.Trigger(eventName);
		MenuSounds.PlayClickSound();
	}
}