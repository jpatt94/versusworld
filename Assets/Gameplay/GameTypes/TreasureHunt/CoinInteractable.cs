using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinInteractable : Interactable
{
	/**********************************************************/
	// Interface

	public override void OnInteract()
	{
		base.OnInteract();

		Destroy(transform.parent.gameObject);
	}
}
