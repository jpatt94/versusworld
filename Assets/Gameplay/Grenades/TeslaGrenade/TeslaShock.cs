using UnityEngine;
using System.Collections;

public class TeslaShock : MonoBehaviour
{
	[SerializeField]
	private float lineChangeInterval;
	[SerializeField]
	private float lineVariance;
	[SerializeField]
	private AnimationCurve lineCurve;
	[SerializeField]
	private AudioClip shockSound;

	private NetworkPlayer victim;
	private NetworkPlayer origin;
	private float nextLineChange;
	private Vector3[] startPos;
	private bool changedLine;

	private LineRenderer line;
	private Transform[] points;
	private AudioSource aud;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		line = GetComponent<LineRenderer>();

		points = new Transform[transform.childCount];
		startPos = new Vector3[transform.childCount];
		for (int i = 0; i < transform.childCount; i++)
		{
			points[i] = transform.GetChild(i);
		}
	}

	public void Update()
	{
		if (!aud)
		{
			aud = GetComponentInParent<AudioSource>();
		}

		if (origin)
		{
			transform.position = origin.Model.HeadPosition;
		}
		else
		{
			transform.localPosition = Vector3.zero;
		}

		Vector3 victimPos = victim.Model.HeadPosition;
		line.SetPosition(0, transform.position);
		line.SetPosition(line.positionCount - 1, victimPos);

		nextLineChange -= Time.deltaTime;
		if (nextLineChange <= 0.0f)
		{
			for (int i = 0; i < points.Length; i++)
			{
				float alpha = (float)(i + 1) / (line.positionCount - 1);
				points[i].localPosition = Vector3.forward * alpha + Random.insideUnitSphere * lineVariance;
			}

			changedLine = true;
			nextLineChange = lineChangeInterval;

			aud.PlayOneShot(shockSound);
		}

		transform.forward = (victimPos - transform.position).normalized;
		transform.localScale = Vector3.one * (victimPos - transform.position).magnitude;
	}

	public void LateUpdate()
	{
		if (changedLine)
		{
			for (int i = 0; i < startPos.Length; i++)
			{
				startPos[i] = points[i].transform.position;
			}

			changedLine = false;
		}

		for (int i = 0; i < points.Length; i++)
		{
			line.SetPosition(i + 1, Vector3.Lerp(startPos[i], points[i].position, 1.0f - (nextLineChange / lineChangeInterval)));
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public NetworkPlayer Victim
	{
		get
		{
			return victim;
		}
		set
		{
			victim = value;
		}
	}

	public NetworkPlayer Origin
	{
		set
		{
			origin = value;
		}
	}
}