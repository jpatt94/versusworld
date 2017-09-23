using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuButton))]
public class MenuButtonEditor : Editor
{
	//SerializedProperty scaleRate;

	/**********************************************************/
	// MonoBehaviour Interface

	public void OnEnable()
	{
		//scaleRate = serializedObject.FindProperty("scaleRate");
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		//serializedObject.Update();

		MenuButton t = (MenuButton)target;
		//EditorGUILayout.PropertyField(scaleRate);

		//serializedObject.ApplyModifiedProperties();
	}
}
