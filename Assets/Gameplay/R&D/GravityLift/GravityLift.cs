using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityLift : MonoBehaviour
{
	[SerializeField]
	private float force;

	/**********************************************************/
	// MonoBehaviour Interface

	public void OnTriggerStay(Collider other)
	{
		OfflineCharacterController con = other.GetComponent<OfflineCharacterController>();
		if (con)
		{
			con.Velocity += Vector3.up * force * Time.fixedDeltaTime;
		}
		else if (other.GetComponent<Rigidbody>())
		{
			other.GetComponent<Rigidbody>().velocity += Vector3.up * force * Time.fixedDeltaTime;
		}
	}
}