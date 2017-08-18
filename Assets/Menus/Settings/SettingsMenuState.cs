using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenuState : MonoBehaviour
{
	protected CanvasRenderer[] canvasRenderers;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		canvasRenderers = GetComponentsInChildren<CanvasRenderer>();
	}

	public virtual void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public virtual void StateBegin()
	{
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		set
		{
			foreach (CanvasRenderer r in canvasRenderers)
			{
				r.gameObject.SetActive(value);
			}
		}
	}
}
