using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerOnlyCollider : MonoBehaviour
{
	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		gameObject.layer = LayerMask.NameToLayer("ControllerOnly");
		if (GetComponent<MeshRenderer>())
		{
			GetComponent<MeshRenderer>().enabled = false;
		}
	}
}
