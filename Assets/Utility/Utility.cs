using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Xml;

public class Utility : MonoBehaviour
{
	public static GameObject FindChild(GameObject parent, string name)
	{
		Transform[] transforms = parent.GetComponentsInChildren<Transform>();
		foreach (Transform t in transforms)
		{
			if (t.gameObject.name == name)
			{
				return t.gameObject;
			}
		}

		return null;
	}

	public static void SetRGB(Text text, Color color)
	{
		text.color = new Color(color.r, color.g, color.b, text.color.a);
	}

	public static void SetRGB(Image image, Color color)
	{
		image.color = new Color(color.r, color.g, color.b, image.color.a);
	}

	public static void SetRGB(Material mat, Color color)
	{
		mat.color = new Color(color.r, color.g, color.b, mat.color.a);
	}

	public static void SetAlpha(Text text, float alpha)
	{
		text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
	}

	public static void SetAlpha(Image image, float alpha)
	{
		image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
	}

	public static void SetAlpha(RawImage image, float alpha)
	{
		image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
	}

	public static float CalculateStringWidth(Text text, string str)
	{
		float width = 0.0f;

		text.font.RequestCharactersInTexture(str, text.fontSize);

		CharacterInfo charInfo = new CharacterInfo();
		foreach (char c in str)
		{
			text.font.GetCharacterInfo(c, out charInfo, text.fontSize);
			width += charInfo.advance;
		}

		return width;
	}

	public static float GetScaleFactorX()
	{
		return Screen.width / 1280.0f;
	}

	public static float GetScaleFactorY()
	{
		return Screen.height / 720.0f;
	}

	public static Vector3 GetCanvasRelativeMouseCoords()
	{
		return new Vector3((Input.mousePosition.x / Screen.width) * 1280.0f, (Input.mousePosition.y / Screen.height) * 720.0f, Input.mousePosition.z);
	}

	public static void EnableButton(Button button, bool enable)
	{
		button.enabled = enable;
		button.GetComponent<Image>().enabled = enable;
		button.GetComponentInChildren<Text>().enabled = enable;
	}

	public static string GetRandomName()
	{
		string[] cons =
		{
			"qu", "w", "r", "t", "y", "p", "s", "d", "f", "g", "h", "j", "k", "l", "z", "c", "v", "b", "n", "m",
			"tr", "th", "pr", "sh", "st", "gh", "fr", "dr", "kl", "zh", "cr", "ch", "br",
		};

		string[] vowels =
		{
			"a", "e", "i", "o", "u", "ou", "oo", "ue", "ee", "ea", "io", "ie", "ei",
		};

		string name = "";

		bool vowel = UnityEngine.Random.Range(0, 2) == 0;
		int length = UnityEngine.Random.Range(3, 6);
		for (int i = 0; i < length; i++)
		{
			if (vowel)
			{
				name += vowels[UnityEngine.Random.Range(0, vowels.Length)];
			}
			else
			{
				name += cons[UnityEngine.Random.Range(0, cons.Length)];
			}

			vowel = !vowel;
		}

		name = name.ToCharArray()[0].ToString().ToUpper() + name.Substring(1);

		return name;
	}

	public static string GetSettingsDirectory()
	{
		return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/VersusWorld";
	}

	public static void XMLSaveInt(XmlDocument doc, XmlNode parent, string name, int value)
	{
		XmlNode node = doc.CreateElement(name);
		XmlAttribute attr = doc.CreateAttribute("Value");
		attr.Value = value.ToString();
		node.Attributes.Append(attr);
		parent.AppendChild(node);
	}

	public static void XMLSaveBool(XmlDocument doc, XmlNode parent, string name, bool value)
	{
		XmlNode node = doc.CreateElement(name);
		XmlAttribute attr = doc.CreateAttribute("Value");
		attr.Value = value ? "TRUE" : "FALSE";
		node.Attributes.Append(attr);
		parent.AppendChild(node);
	}

	public static void XMLLoadInt(XmlNode parent, string name, out int value)
	{
		XmlNode node = parent.SelectSingleNode(name);
		value = System.Convert.ToInt32(node.Attributes["Value"].Value);
	}

	public static void XMLLoadBool(XmlNode parent, string name, out bool value)
	{
		XmlNode node = parent.SelectSingleNode(name);
		value = node.Attributes["Value"].Value == "TRUE";
	}
}
