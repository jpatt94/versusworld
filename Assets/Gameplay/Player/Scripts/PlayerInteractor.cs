using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
	[SerializeField]
	private float range;

	private bool interactableAvailable;
	private int layer;

	private CameraManager cam;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		layer = ~((1 << LayerMask.NameToLayer("LocalBodyPartCollider") | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("PlayerController"))));

		cam = GetComponentInChildren<CameraManager>();
	}

	public void Update()
	{
		RaycastHit hit;
		Physics.Raycast(new Ray(cam.WorldCamera.transform.position, cam.WorldCamera.transform.forward), out hit, range, layer, QueryTriggerInteraction.Collide);

		if (hit.collider)
		{
			Interactable i = hit.collider.GetComponent<Interactable>();
			if (i && i.enabled)
			{
				interactableAvailable = true;
				HUD.Instance.WeaponPickUp.Enable(i.DisplayText);

				if (PlayerInput.Interact())
				{
					i.OnInteract();
				}
			}
		}
		else
		{
			interactableAvailable = false;
		}
	}

	/**********************************************************/
	// Accessors/Mutators

	public bool InteractableAvailable
	{
		get
		{
			return interactableAvailable;
		}
	}
}
