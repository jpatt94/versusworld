using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerNetworkTransform : NetworkTransform
{
	//private Respawner respawn;

	void Start()
	{
		clientMoveCallback3D = ValidateMove;
		//respawn = GetComponent<Respawner>();
	}

	private bool ValidateMove(ref Vector3 position, ref Vector3 velocity, ref Quaternion rotation)
	{
		//if (respawn.enabled)
		//{
		//	if ((respawn.RespawnPosition - position).sqrMagnitude < 1.0f)
		//	{
		//		respawn.AckClientUpdate();
		//		return true;
		//	}
		//	else
		//	{
		//		return false;
		//	}
		//}

		return true;
	}
}
