using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keybind
{
	private KeyCode key;
	private int mouseButton;
	private int mouseWheel;

	public Keybind()
	{
		key = KeyCode.None;
		mouseButton = -1;
		mouseWheel = 0;
	}

	/**********************************************************/
	// Interface

	public override string ToString()
	{
		if (key != KeyCode.None)
		{
			string name = key.ToString();
			for (int i = 1; i < name.Length; i++)
			{
				if (name[i] >= 'A' && name[i] <= 'Z')
				{
					name = name.Insert(i, " ");
					i++;
				}
			}
			return name;
		}
		else if (mouseButton > -1)
		{
			return "Mouse Button " + mouseButton.ToString();
		}
		else if (mouseWheel != 0)
		{
			return mouseWheel < 0 ? "Mouse Wheel Up" : "Mouse Wheel Down";
		}

		return "N/A";
	}

	/**********************************************************/
	// Accessors/Mutators

	public KeyCode Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;

			if (value != KeyCode.None)
			{
				mouseButton = -1;
				mouseWheel = 0;
			}
		}
	}

	public int MouseButton
	{
		get
		{
			return mouseButton;
		}
		set
		{
			mouseButton = value;

			if (value > -1)
			{
				key = KeyCode.None;
				mouseWheel = 0;
			}
		}
	}

	public int MouseWheel
	{
		get
		{
			return mouseWheel;
		}
		set
		{
			mouseWheel = value;

			if (value != 0)
			{
				key = KeyCode.None;
				mouseButton = -1;
			}
		}
	}
}
