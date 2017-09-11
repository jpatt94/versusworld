using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GlassObject : NetworkBehaviour
{
	/**********************************************************/
	// Mono/NetworkBehaviour Interface

	public void Awake()
	{
		Transform t = transform.Find("Glass");
		for (int i = 0; i < int.MaxValue; i++)
		{
			Transform glass = t.Find("Glass" + i);
			if (glass)
			{
				if (NetworkServer.active)
				{
					GameObject obj = Instantiate(EnvironmentManager.GlassPrefab);
					obj.transform.position = glass.transform.position;
					obj.transform.rotation = glass.transform.rotation;
					obj.transform.localScale = glass.transform.localScale;
					obj.GetComponent<Glass>().Scale = glass.transform.localScale;
					NetworkServer.Spawn(obj);
				}

				Destroy(glass.gameObject);
			}
			else
			{
				break;
			}
		}
	}
}
