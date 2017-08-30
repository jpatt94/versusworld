using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[ExecuteInEditMode]
public class VolumeRolloff : MonoBehaviour
{
	[SerializeField]
	private Vector2[] keyframes;

	/**********************************************************/
	// Interface

	public void Generate()
	{
		AnimationCurve curve = new AnimationCurve();
		foreach (Vector2 k in keyframes)
		{
			Keyframe keyframe = new Keyframe();
			keyframe.time = k.x;
			keyframe.value = k.y;
			curve.AddKey(keyframe);
		}
		for (int i = 0; i < curve.keys.Length; i++)
		{
		}
		for (int i = 1; i < curve.keys.Length - 1; i++)
		{
			curve.keys[i].outTangent = curve.keys[i + 1].inTangent + Mathf.PI / 2;
		}

		GetComponent<AudioSource>().SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
	}
}
