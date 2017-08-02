using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
	[SerializeField]
	private GameObject test1;
	[SerializeField]
	private GameObject test2;

	private Animator ani;

	/**********************************************************/
	// MonoBehaviour Interface

	public void Awake()
	{
		ani = GetComponentInChildren<Animator>();
	}

	public void OnAnimatorIK()
	{
		//ani.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
		//ani.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
		//
		//ani.SetIKPosition(AvatarIKGoal.RightHand, test1.transform.position);
		//ani.SetIKRotation(AvatarIKGoal.RightHand, test1.transform.rotation);
		//
		//ani.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
		//ani.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
		//
		//ani.SetIKPosition(AvatarIKGoal.LeftHand, test2.transform.position);
		//ani.SetIKRotation(AvatarIKGoal.LeftHand, test2.transform.rotation);
	}
}
