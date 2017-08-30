using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VolumeRolloff))]
public class VolumeRolloffEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Generate"))
		{
			VolumeRolloff script = (VolumeRolloff)target;
			script.Generate();
		}
	}
}
