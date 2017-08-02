using UnityEngine;
using System.Collections;

public class RespawnCamera : MonoBehaviour
{
	public AnimationCurve zoomOutCurve;
	public float zoomOutDistance;
	public float zoomOutDuration;
	public float minXRotation;
	public float maxXRotation;
	public float minDistanceFromCollider;

	private Vector3 startingPos;
	private Vector3 rotation;
	private float time;

	private Camera cam;
	private AudioListener aud;

	void Awake()
	{
		cam = GetComponent<Camera>();
		aud = GetComponent<AudioListener>();
	}

	void Update()
	{
		time += Time.deltaTime;

		rotation.y += PlayerInput.MouseLookAxis.x;
		rotation.x += PlayerInput.MouseLookAxis.y;
		if (rotation.x < minXRotation)
		{
			rotation.x = minXRotation;
		}
		else if (rotation.x > maxXRotation)
		{
			rotation.x = maxXRotation;
		}

		float zoom = zoomOutDistance * zoomOutCurve.Evaluate(time / zoomOutDuration) + minDistanceFromCollider;
		Vector3 targetPos = Vector3.zero;

		float arcY = Mathf.Sin(rotation.x * Mathf.Deg2Rad) * zoom;
		float arcX = Mathf.Cos(rotation.x * Mathf.Deg2Rad) * zoom;

		targetPos.x = startingPos.x + Mathf.Cos(rotation.y * Mathf.Deg2Rad) * arcX;
		targetPos.y = startingPos.y + arcY;
		targetPos.z = startingPos.z + Mathf.Sin(rotation.y * Mathf.Deg2Rad) * arcX;

		Vector3 toTarget = (targetPos - startingPos).normalized;
		RaycastHit hit;
		if (Physics.Raycast(new Ray(startingPos, toTarget), out hit, zoom))
		{
			zoom = hit.distance - minDistanceFromCollider;
		}

		transform.position = startingPos + toTarget * zoom;
		transform.forward = -toTarget;
	}

	/**********************************************************/
	// Interface

	public void Enable(bool _enabled, CameraManager playerCam)
	{
		enabled = _enabled;
		cam.enabled = _enabled;
		aud.enabled = _enabled;

		if (enabled)
		{
			transform.position = playerCam.transform.position;
			transform.rotation = playerCam.transform.rotation;
			startingPos = transform.position;
			rotation = playerCam.transform.rotation.eulerAngles;
			time = 0.0f;

			if (rotation.x < minXRotation)
			{
				rotation.x = minXRotation;
			}
			else if (rotation.x > maxXRotation)
			{
				rotation.x = maxXRotation;
			}
		}
	}
}
