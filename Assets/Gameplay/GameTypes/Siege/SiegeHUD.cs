using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SiegeHUD : MonoBehaviour
{
	[SerializeField]
	private Color contestingColor;

	private CaptureStatus captureStatus;
	private int localTeam;
	private int otherTeam;

	private Text captureText;
	private Text speedText;
	private Slider captureSlider;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		localTeam = PartyManager.LocalPlayer.Team;
		otherTeam = localTeam == 0 ? 1 : 0;

		captureText = transform.Find("CaptureText").GetComponent<Text>();
		speedText = transform.Find("SpeedText").GetComponent<Text>();
		captureSlider = transform.Find("CaptureSlider").GetComponent<Slider>();

		SiegeSettings settings = PartyManager.GameSettings as SiegeSettings;
		speedText.text = settings.MultipleCapturersSpeed.ToString("0.0") + "x";
		speedText.enabled = false;
	}

	/**********************************************************/
	// Interface

	public void OnGameOver()
	{
		captureText.enabled = false;
		speedText.enabled = false;

		foreach (CanvasRenderer r in captureSlider.GetComponentsInChildren<CanvasRenderer>())
		{
			r.cull = true;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public CaptureStatus CaptureStatus
	{
		set
		{
			CaptureStatus prevStatus = captureStatus;
			captureStatus = value;

			if (prevStatus != captureStatus)
			{
				if (captureStatus == CaptureStatus.Capturing)
				{
					captureText.text = "Capturing";
					Utility.SetRGB(captureText, PartyManager.GetTeamColor(localTeam));
					Utility.SetRGB(speedText, PartyManager.GetTeamColor(localTeam));
				}
				else if (captureStatus == CaptureStatus.Contesting)
				{
					captureText.text = "Contesting";
					Utility.SetRGB(captureText, contestingColor);
				}
				else if (captureStatus == CaptureStatus.LosingPoint)
				{
					captureText.text = "Losing Point";
					Utility.SetRGB(captureText, PartyManager.GetTeamColor(otherTeam));
					Utility.SetRGB(speedText, PartyManager.GetTeamColor(otherTeam));
				}
				else
				{
					captureText.text = "";
				}
			}
		}
	}

	public float CaptureAmount
	{
		get
		{
			return captureSlider.value;
		}
		set
		{
			captureSlider.value = value;
		}
	}

	public bool SpeedMult
	{
		set
		{
			speedText.enabled = value;
		}
	}
}

public enum CaptureStatus
{
	None,
	Capturing,
	Contesting,
	LosingPoint,
}