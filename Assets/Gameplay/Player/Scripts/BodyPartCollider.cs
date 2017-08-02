using UnityEngine;
using System.Collections;

public class BodyPartCollider : MonoBehaviour
{
	public BodyPart type;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		gameObject.layer = LayerMask.NameToLayer("BodyPartCollider");
	}
}

public enum BodyPart
{
	Head,
	UpperTorso,
	LowerTorso,
	RightUpperArm,
	RightForearm,
	LeftUpperArm,
	LeftForearm,
	RightUpperLeg,
	RightLowerLeg,
	RightFoot,
	LeftUpperLeg,
	LeftLowerLeg,
	LeftFoot,
	None,
}