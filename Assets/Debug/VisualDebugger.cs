using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class VisualDebugger : MonoBehaviour
{
	private Canvas canvas;
    private List<string> entries;
    private Text variableTracking;

    static VisualDebugger instance = null;

    void Start()
    {
		canvas = GetComponent<Canvas>();
		canvas.enabled = false;
        entries = new List<string>();
		variableTracking = Utility.FindChild(gameObject, "VariableTracking").GetComponent<Text>();
        instance = this;
    }

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			canvas.enabled = !canvas.enabled;
		}
	}

    void LateUpdate()
    {
        if (entries.Count > 0)
        {
			variableTracking.text = "";

            foreach (string s in entries)
            {
				variableTracking.text += s;
				variableTracking.text += "\n";
            }
        }

        entries.Clear();
    }

	/**********************************************************/
	// Interface

	public static void TrackVariable(string variableName, object variable)
    {
        if (instance != null)
        {
            for (int i = 0; i < instance.entries.Count; i++)
            {
                string s = instance.entries[i];
                string varName = "";
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[j] == ':')
                    {
                        break;
                    }

                    varName += s[j];
                }

                if (varName == variableName)
                {
                    instance.entries[i] = variableName + ": " + variable.ToString();
                    return;
                }
            }

            instance.entries.Add(variableName + ": " + variable.ToString());
        }
    }

	public static bool Enabled
	{
		get
		{
			return instance.canvas.enabled;
		}
		set
		{
			instance.canvas.enabled = value;
		}
	}
}
