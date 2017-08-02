using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WeaponPickUp : MonoBehaviour
{
	[SerializeField]
	private float popDuration;
	[SerializeField]
	private float popScale;
	[SerializeField]
	private AnimationCurve popCurve;

	private WeaponType prevWeaponType;
	private float popTime;

	private Text text;
	private Image weaponIcon;
	private HUD mgr;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		text = GetComponent<Text>();
		weaponIcon = GetComponentInChildren<Image>();
		mgr = GetComponentInParent<HUD>();

		Disable();
	}

	public void Update()
	{
		popTime += Time.deltaTime;
		if (text.enabled)
		{
			transform.localScale = Vector3.one + Vector3.one * popCurve.Evaluate(popTime / popDuration) * popScale;
		}
	}

	/**********************************************************/
	// Interface

	public void Enable(WeaponType type)
	{
		text.enabled = true;
		text.text = "Hold Q to swap for";
		weaponIcon.enabled = true;
		weaponIcon.sprite = mgr.AmmoDisplay.WeaponIcons[(int)type];

		if (type != prevWeaponType)
		{
			Pop();
		}

		prevWeaponType = type;
	}

	public void Enable(string str)
	{
		text.enabled = true;
		text.text = str;
		weaponIcon.enabled = false;
	}

	public void Disable()
	{
		text.enabled = false;
		weaponIcon.enabled = false;
		prevWeaponType = WeaponType.None;
	}

	/**********************************************************/
	// Helper Functions

	private void Pop()
	{
		popTime = 0.0f;
	}
}
