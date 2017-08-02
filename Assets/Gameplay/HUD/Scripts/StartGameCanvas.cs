using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGameCanvas : MonoBehaviour
{
	private Image background;
	private Text text;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		background = transform.Find("Background").GetComponent<Image>();
		text = transform.Find("Text").GetComponent<Text>();
	}
}