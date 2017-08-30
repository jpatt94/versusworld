using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	[SerializeField]
	private float defaultFOV;
	[SerializeField]
	private float minimumXRotation = -85.0f;
	[SerializeField]
	private float maximumXRotaiton = 85.0f;
	[SerializeField]
	private AnimationCurve headBobXCurve;
	[SerializeField]
	private AnimationCurve headBobYCurve;
	[SerializeField]
	private float headBobXHeight;
	[SerializeField]
	private float headBobYHeight;
	[SerializeField]
	private float headBobSpeed;
	[SerializeField]
	private float headBobTransitionSpeed;
	[SerializeField]
	private float handsPositionLagAmount;
	[SerializeField]
	private float crouchingYOffset;
	[SerializeField]
	private float crouchTransitionSpeed;
	[SerializeField]
	private AnimationCurve crouchTransitionCurve;
	[SerializeField]
	private AnimationCurve jerkCurve;
	[SerializeField]
	private float headShotJerkFactor;
	[SerializeField]
	private float headShotJerkDuration;
	[SerializeField]
	private AnimationCurve headShotJerkDamageCurve;
	[SerializeField]
	private AnimationCurve zShakeCurve;
	[SerializeField]
	private float hitZShakeFactor;
	[SerializeField]
	private float hitZShakeDuration;
	[SerializeField]
	private AnimationCurve viewShakeCurve;

	private Quaternion playerRotation;
	private Quaternion cameraRotation;
	private bool headBobEnabled;
	private float headBobTime;
	private float headBobStopTime;
	private float headBobAlpha;
	private bool crouching;
	private float startingYOffset;
	private float crouchAlpha;
	private Vector2 jerk;
	private float jerkTime;
	private float prevJerkTime;
	private float jerkDuration;
	private bool jerkUsesZShakeCurve;
	private float zShake;
	private float zShakeTime;
	private float zShakeDuration;
	private Vector3 viewShake;
	private float viewShakeTime;
	private float viewShakeDuration;

	private Transform view;
	private Camera cam;
	private Camera firstPersonCam;
	private AudioListener aud;
	private Transform playerTransform;
	private OfflinePlayerModel playerModel;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		view = transform.Find("View");
		cam = view.Find("Camera").GetComponent<Camera>();
		firstPersonCam = view.Find("FirstPersonCamera").GetComponent<Camera>();
		aud = GetComponent<AudioListener>();
		playerModel = GetComponentInParent<OfflinePlayerModel>();

		AudioListener.volume = 0.6f;
	}

	public void Start()
	{
		playerTransform = GetComponentInParent<OfflineCharacterController>().GetComponent<Transform>();

		playerRotation = playerTransform.localRotation;
		cameraRotation = transform.localRotation;
		headBobEnabled = false;
		headBobTime = 0.0f;
		headBobStopTime = 0.0f;
		headBobAlpha = 0.0f;
		startingYOffset = transform.localPosition.y;
	}

	public void OnEnable()
	{
		HUD.Instance.ShakeCanvas.transform.SetParent(transform);
		HUD.Instance.ShakeCanvas.transform.localPosition = Vector3.forward * 500.0f;
		HUD.Instance.ShakeCanvas.transform.localRotation = Quaternion.identity;

		HUD.Instance.Camera.transform.SetParent(view);
		HUD.Instance.Camera.transform.localPosition = Vector3.zero;
	}

	public void Update()
	{
		UpdateJerk();
		UpdateMouseLook();
		UpdateHeadBob();
		UpdateZShake();
		UpdateCrouch();
		UpdateViewShake();

		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
		{
			if (AudioListener.volume > 0.0f)
			{
				AudioListener.volume = 0.0f;
			}
			else
			{
				AudioListener.volume = 0.6f;
			}
		}

		//VisualDebugger.TrackVariable("FPS", Mathf.RoundToInt(1.0f / Time.deltaTime));
	}

	/**********************************************************/
	// Interface

	public void Enable(bool _enabled)
	{
		if (_enabled && !enabled)
		{
			jerkDuration = 0.0f;
			zShakeDuration = 0.0f;
		}

		enabled = _enabled;
		cam.enabled = _enabled;
		firstPersonCam.enabled = _enabled;
		aud.enabled = _enabled;
	}

	public void SetRotation(Quaternion player, Quaternion camera)
	{
		playerRotation = player;
		cameraRotation = camera;
	}

	public void SetFOV(float fov)
	{
		cam.fieldOfView = fov;
	}

	public void SetHeadBob(bool enable)
	{
		if (enable != headBobEnabled && headBobStopTime - headBobTime < 2.0f)
		{
			headBobStopTime = Mathf.Round(headBobStopTime + 2.0f);
		}

		headBobEnabled = enable;
	}

	public float GetLookRotationAlpha()
	{
		float camRotX = transform.localRotation.eulerAngles.x;
		if (camRotX > 180.0f)
		{
			camRotX -= 360.0f;
		}
		return Mathf.InverseLerp(minimumXRotation, maximumXRotaiton, camRotX);
	}

	public void Jerk(Vector2 jerk, float duration, bool useZShakeCurve = false)
	{
		this.jerk = jerk;
		jerkTime = 0.0f;
		prevJerkTime = 0.0f;
		jerkDuration = duration;
		jerkUsesZShakeCurve = useZShakeCurve;
	}

	public void ZShake(float zShake, float duration)
	{
		this.zShake = zShake;
		zShakeTime = 0.0f;
		zShakeDuration = duration;
	}

	public void OnTakeDamage(float damage, Vector3 position, BodyPart bodyPart)
	{
		Vector3 toHit = position - transform.position;
		toHit.y = 0.0f;
		toHit.Normalize();
		float toHitAngle = Mathf.Atan2(toHit.z, toHit.x);

		Vector3 camForward = transform.forward;
		camForward.y = 0.0f;
		camForward.Normalize();
		float camForwardAngle = Mathf.Atan2(camForward.z, camForward.x);

		Vector3 relativeToHit = Quaternion.Euler(0.0f, (toHitAngle - camForwardAngle) * Mathf.Rad2Deg, 0.0f) * Vector3.forward;

		float damageRatio = headShotJerkDamageCurve.Evaluate(damage);
		if (bodyPart == BodyPart.Head)
		{
			Jerk(new Vector2(-headShotJerkFactor * relativeToHit.z, headShotJerkFactor * relativeToHit.x) * damageRatio, headShotJerkDuration);
		}
		ZShake(hitZShakeFactor * -relativeToHit.x, hitZShakeDuration);
	}

	public void ViewShake(float amount, float duration)
	{
		viewShake = Random.onUnitSphere * amount;
		viewShakeTime = 0.0f;
		viewShakeDuration = duration;
	}

	public void ViewShake(Vector3 amount, float duration)
	{
		viewShake = amount;
		viewShakeTime = 0.0f;
		viewShakeDuration = duration;
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateJerk()
	{
		if (jerkTime < jerkDuration)
		{
			jerkTime += Time.deltaTime;
			float delta;
			if (jerkUsesZShakeCurve)
			{
				delta = zShakeCurve.Evaluate(jerkTime / jerkDuration) - zShakeCurve.Evaluate(prevJerkTime / jerkDuration);
			}
			else
			{
				delta = jerkCurve.Evaluate(jerkTime / jerkDuration) - jerkCurve.Evaluate(prevJerkTime / jerkDuration);
			}
			prevJerkTime = jerkTime;

			playerRotation *= Quaternion.Euler(0.0f, jerk.y * delta, 0.0f);
			cameraRotation *= Quaternion.Euler(jerk.x * delta, 0.0f, 0.0f);
		}
	}

	private void UpdateZShake()
	{
		if (zShakeTime < zShakeDuration)
		{
			zShakeTime += Time.deltaTime;

			Vector3 rot = cam.transform.localRotation.eulerAngles;
			rot.z = zShake * zShakeCurve.Evaluate(zShakeTime / zShakeDuration);
			cam.transform.localRotation = Quaternion.Euler(rot);
			firstPersonCam.transform.localRotation = cam.transform.localRotation;
		}
	}

	private void UpdateMouseLook()
	{
		Vector2 lookAxis = PlayerInput.MouseLookAxis;

		playerRotation *= Quaternion.Euler(0.0f, lookAxis.x, 0.0f);
		cameraRotation *= Quaternion.Euler(-lookAxis.y, 0.0f, 0.0f);

		cameraRotation = ClampRotationAroundXAxis(cameraRotation);

		playerTransform.localRotation = playerRotation;
		transform.localRotation = cameraRotation;

		playerModel.FirstPersonHands.AddPositionLag(-lookAxis * handsPositionLagAmount);
	}

	private void UpdateHeadBob()
	{
		if (headBobTime != headBobStopTime)
		{
			headBobTime += Time.deltaTime * headBobSpeed;
		}

		if (headBobTime >= headBobStopTime - Mathf.Epsilon)
		{
			if (headBobEnabled)
			{
				headBobStopTime = Mathf.Round(headBobStopTime + 2.0f);
			}
			else
			{
				headBobTime = headBobStopTime;
			}
		}

		headBobAlpha = Mathf.Clamp01(headBobAlpha + Time.deltaTime * headBobTransitionSpeed * (headBobEnabled ? 1.0f : -1.0f));

		float xRot = headBobXCurve.Evaluate(headBobTime) * headBobXHeight * headBobAlpha;
		float yRot = headBobYCurve.Evaluate(headBobTime) * headBobYHeight * headBobAlpha;

		cam.transform.localRotation = Quaternion.Euler(xRot, yRot, 0.0f);
		firstPersonCam.transform.localRotation = cam.transform.localRotation;
	}

	private Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
		angleX = Mathf.Clamp(angleX, minimumXRotation, maximumXRotaiton);
		q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

	private void UpdateCrouch()
	{
		crouchAlpha = Mathf.Clamp01(crouchAlpha + Time.deltaTime * crouchTransitionSpeed * (crouching ? 1.0f : -1.0f));

		Vector3 newPos = transform.localPosition;
		newPos.y = Mathf.Lerp(startingYOffset, crouchingYOffset, crouchTransitionCurve.Evaluate(crouchAlpha));
		transform.localPosition = newPos;
	}

	private void UpdateViewShake()
	{
		if (viewShakeTime < viewShakeDuration)
		{
			viewShakeTime += Time.deltaTime;
			view.localRotation = Quaternion.Euler(viewShake * viewShakeCurve.Evaluate(viewShakeTime / viewShakeDuration));
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public float DefaultFOV
	{
		get
		{
			return defaultFOV;
		}
	}

	public bool Crouching
	{
		get
		{
			return crouching;
		}
		set
		{
			crouching = value;
		}
	}

	public Camera FirstPersonCamera
	{
		get
		{
			return firstPersonCam;
		}
	}

	public Camera WorldCamera
	{
		get
		{
			return cam;
		}
	}
}
