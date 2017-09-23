using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
	[SerializeField]
	private float duration;
	[SerializeField]
	private float mass;
	[SerializeField]
	private Mesh defaultMesh;
	[SerializeField]
	private Mesh headlessMesh;
	[SerializeField]
	private GameObject headPrefab;

	private SkinnedMeshRenderer mesh;
	private ParticleSystem neckBloodEffect;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		Destroy(gameObject, duration);

		mesh = GetComponentInChildren<SkinnedMeshRenderer>();
		mesh.sharedMesh = defaultMesh;

		foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
		{
			r.mass = mass;
		}
	}

	/**********************************************************/
	// Interface

	public void ApplyForce(Vector3 force)
	{
		foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
		{
			r.AddForce(force, ForceMode.Impulse);
		}
	}

	public Transform SeparateHead(Vector3 force)
	{
		mesh.sharedMesh = headlessMesh;

		GameObject head = Instantiate(headPrefab);
		Transform headTransform = transform.Find("Bro_Reference/Bro_Hips/Bro_Spine/Bro_Spine1/Bro_Spine2/Bro_Neck").transform;
		head.transform.position = headTransform.position;
		head.transform.rotation = headTransform.rotation;
		head.transform.localScale = headTransform.localScale;
		head.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
		head.GetComponent<Rigidbody>().maxDepenetrationVelocity = force.magnitude;
		head.GetComponentInChildren<MeshRenderer>().material = mesh.material;
		Destroy(head, duration);

		transform.Find("Bro_Reference/Bro_Hips/Bro_Spine/Bro_Spine1/Bro_Spine2/NeckBloodEffect").GetComponent<ParticleSystem>().Play();

		return head.transform.Find("Pivot");
	}
}