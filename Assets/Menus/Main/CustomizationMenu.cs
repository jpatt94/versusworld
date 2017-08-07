using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationMenu : MonoBehaviour
{
	[SerializeField]
	private Vector3 idleRotation;
	[SerializeField]
	private Vector3 activeRotation;
	[SerializeField]
	private float transitionDuration;
	[SerializeField]
	private AnimationCurve transitionCurve;

	private float transTime;

	private LegacyMainMenu mainMenu;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		mainMenu = GetComponentInParent<LegacyMainMenu>();
	}

	public void Update()
	{
		transTime += Time.deltaTime * (mainMenu.State == MenuType.Customize ? 1 : -1);
		transTime = Mathf.Clamp(transTime, 0.0f, transitionDuration);

		float alpha = transitionCurve.Evaluate(transTime / transitionDuration);
		transform.localRotation = Quaternion.Lerp(Quaternion.Euler(idleRotation), Quaternion.Euler(activeRotation), alpha);
	}
}