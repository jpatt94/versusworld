using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackedMenuState : MonoBehaviour
{
	protected CanvasRenderer[] canvasRenderers;

	/**********************************************************/
	// MonoBehaviour Interface

	public virtual void Awake()
	{
		canvasRenderers = GetComponentsInChildren<CanvasRenderer>();
	}

	public virtual void Start()
	{
		Visible = false;
	}

	public virtual void Update()
	{

	}

	public virtual void OnDestroy()
	{
		JP.Event.UnregisterAll(this);
	}

	/**********************************************************/
	// Interface

	public virtual void StateBegin()
	{
		Visible = true;
	}

	public virtual void StateUpdate()
	{

	}

	public virtual void StateEnd()
	{
		Visible = false;
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool Visible
	{
		get
		{
			return canvasRenderers[0].gameObject.activeInHierarchy;
		}
		set
		{
			foreach (CanvasRenderer r in canvasRenderers)
			{
				r.gameObject.SetActive(value);
			}
		}
	}
}
