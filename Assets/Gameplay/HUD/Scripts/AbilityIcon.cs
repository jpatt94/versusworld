using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityIcon : MonoBehaviour
{
	[SerializeField]
	private float popScale;
	[SerializeField]
	private float popScaleDecreaseRate;

	private AbilityDisplay mgr;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Update()
	{
		transform.localScale = Vector3.one * (Mathf.Max(transform.localScale.x - popScaleDecreaseRate * Time.deltaTime, 1.0f));
	}

	/**********************************************************/
	// Interface

	public void Pop()
	{
		transform.localScale = Vector3.one * popScale;
	}

	/**********************************************************/
	// Accessors/Mutators

	public AbilityDisplay Manager
	{
		set
		{
			mgr = value;
		}
	}

	public bool Visible
	{
		set
		{
			foreach (CanvasRenderer r in GetComponentsInChildren<CanvasRenderer>())
			{
				r.cull = !value;
			}
		}
	}
}
