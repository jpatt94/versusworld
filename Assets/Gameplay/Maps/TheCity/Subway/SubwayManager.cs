using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SubwayManager : SafeNetworkBehaviour
{
	[SerializeField]
	private float speed;

	private SubwayTrain train;
	[SerializeField]
	private AnimationCurve trackXCurve;
	private AnimationCurve trackZCurve;
	private List<float> distanceToNextNode;

	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	protected override bool Ready()
	{
		return FindObjectOfType<SubwayTrain>();
	}

	protected override void DelayedAwake()
	{
		base.DelayedAwake();

		train = GameObject.Find("SubwayTrain").GetComponent<SubwayTrain>();

		trackXCurve = new AnimationCurve();
		trackZCurve = new AnimationCurve();
		trackXCurve.postWrapMode = WrapMode.Loop;
		trackZCurve.postWrapMode = WrapMode.Loop;

		Transform trackNodes = transform.Find("Track");
		for (int i = 0; i < int.MaxValue; i++)
		{
			Transform node = trackNodes.Find("Node" + i);
			if (node)
			{
				trackXCurve.AddKey(i, node.position.x);
				trackZCurve.AddKey(i, node.position.z);
			}
			else
			{
				break;
			}
		}
		trackXCurve.AddKey(trackXCurve.length, trackNodes.Find("Node0").position.x);
		trackZCurve.AddKey(trackZCurve.length, trackNodes.Find("Node0").position.z);

		train.transform.position = trackNodes.Find("Node0").position;

		distanceToNextNode = new List<float>();
		for (int j = 0; j < trackXCurve.length - 1; j++)
		{
			trackXCurve.SmoothTangents(j, 0.0f);
			trackZCurve.SmoothTangents(j, 0.0f);

			int nextIndex = (j == trackXCurve.length - 2) ? 0 : j + 1;
			distanceToNextNode.Add((transform.Find("Track/Node" + nextIndex).position - transform.Find("Track/Node" + j).position).magnitude);
		}
		trackXCurve.SmoothTangents(trackXCurve.length - 1, 0.0f);
		trackZCurve.SmoothTangents(trackZCurve.length - 1, 0.0f);
	}

	public override void Update()
	{
		base.Update();

		if (initialized)
		{
			train.NormalizedTime += (speed / distanceToNextNode[Mathf.FloorToInt(train.NormalizedTime)]) * Time.deltaTime;
			while (train.NormalizedTime >= distanceToNextNode.Count)
			{
				train.NormalizedTime -= distanceToNextNode.Count;
			}
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public AnimationCurve TrackXCurve
	{
		get
		{
			return trackXCurve;
		}
	}

	public AnimationCurve TrackZCurve
	{
		get
		{
			return trackZCurve;
		}
	}
}
