using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	[SerializeField]
	private float speed;
	[SerializeField]
	private float accel;
	[SerializeField]
	private GameObject kinematicPrefab;
	[SerializeField]
	private Mesh[] meshes;
	[SerializeField]
	private Material[] materials;

	private int type;
	private List<Vector3> waypoints;

	private Rigidbody rig;
	private Rigidbody kinematic;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		rig = GetComponent<Rigidbody>();

		GameObject obj = Instantiate(kinematicPrefab);
		kinematic = obj.GetComponent<Rigidbody>();
		Physics.IgnoreCollision(rig.GetComponentInChildren<Collider>(), obj.GetComponentInChildren<Collider>());

		Type = Random.Range(0, meshes.Length);

		waypoints = new List<Vector3>();
		obj = GameObject.Find("TestWaypoints");
		for (int i = 0; i < 1000; i++)
		{
			Transform t = obj.transform.Find("Waypoint" + i);
			if (!t)
			{
				break;
			}
			waypoints.Add(t.position);
		}
	}

	public void FixedUpdate()
	{
		if (waypoints.Count > 0)
		{
			Vector3 toWaypoint = waypoints[0] - rig.position;
			if (toWaypoint.sqrMagnitude < 2.0f)
			{
				waypoints.RemoveAt(0);
				toWaypoint = waypoints[0] - rig.position;
			}

			toWaypoint.Normalize();

			rig.velocity += toWaypoint * accel * Time.fixedDeltaTime;
			if (rig.velocity.sqrMagnitude > speed * speed)
			{
				rig.velocity = rig.velocity.normalized * speed;
			}
		}

		kinematic.MovePosition(rig.position);
		kinematic.MoveRotation(rig.rotation);
	}

	/**********************************************************/
	// Accessors/Mutators

	public int Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			GetComponentInChildren<MeshFilter>().mesh = meshes[value];
			GetComponentInChildren<MeshRenderer>().material = materials[value];
			GetComponentInChildren<MeshCollider>().sharedMesh = meshes[value];
			kinematic.GetComponentInChildren<MeshCollider>().sharedMesh = meshes[value];
		}
	}
}