using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingMapCanvas : MonoBehaviour
{
	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}