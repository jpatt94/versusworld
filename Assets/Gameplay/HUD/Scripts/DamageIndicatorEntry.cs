using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamageIndicatorEntry : MonoBehaviour
{
	public float duration;
	public AnimationCurve alphaCurve;

	private Vector3 shooterPosition;
	private float timeLeft;
	private int shooterID;
	private float startingAlpha;

	private CameraManager cam;
	private DamageIndicator mgr;
	private RawImage image;

	void Awake()
	{
		image = GetComponent<RawImage>();
		startingAlpha = image.color.a;
	}

	void Update()
	{
		UpdateRotation();

		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f)
		{
			mgr.OnFinish(shooterID);
		}

		Utility.SetAlpha(image, startingAlpha * alphaCurve.Evaluate(1.0f - timeLeft / duration));
	}

	/**********************************************************/
	// Interface

	public void Initialize(Vector3 shooterPosition, CameraManager cam, DamageIndicator mgr)
	{
		this.shooterPosition = shooterPosition;
		this.cam = cam;
		this.mgr = mgr;

		timeLeft = duration;

		UpdateRotation();
	}

	/**********************************************************/
	// Helper Functions

	private void UpdateRotation()
	{
		Vector3 toShooter = shooterPosition - cam.transform.position;
		toShooter.y = 0.0f;
		toShooter.Normalize();
		float toShooterAngle = Mathf.Atan2(toShooter.z, toShooter.x);

		Vector3 camForward = cam.transform.forward;
		camForward.y = 0.0f;
		camForward.Normalize();
		float camAngle = Mathf.Atan2(camForward.z, camForward.x);

		Vector3 newRotation = image.rectTransform.localRotation.eulerAngles;
		newRotation.z = (toShooterAngle - camAngle) * Mathf.Rad2Deg;
		image.rectTransform.localRotation = Quaternion.Euler(newRotation);
	}

	/**********************************************************/
	// Accessors/Mutators

	public int ShooterID
	{
		get
		{
			return shooterID;
		}
		set
		{
			shooterID = value;
		}
	}
}
