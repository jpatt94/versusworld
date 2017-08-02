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

	private MainMenu mainMenu;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		mainMenu = GetComponentInParent<MainMenu>();
	}

	public void Update()
	{
		transTime += Time.deltaTime * (mainMenu.State == MenuState.Customize ? 1 : -1);
		transTime = Mathf.Clamp(transTime, 0.0f, transitionDuration);

		float alpha = transitionCurve.Evaluate(transTime / transitionDuration);
		transform.localRotation = Quaternion.Lerp(Quaternion.Euler(idleRotation), Quaternion.Euler(activeRotation), alpha);
	}
}