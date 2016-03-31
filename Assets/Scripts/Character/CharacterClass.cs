using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterClass : MonoBehaviour {

	private List<WeaponBase> playerWeapons;

	private float healthPoints;
	[SerializeField]
	private float walkSpeed;
	[SerializeField]
	private float runSpeed;

	private bool isWalking;

	[SerializeField] [Range(0f, 1f)] 
	private float m_RunstepLenghten;
	[SerializeField] 
	private float m_JumpSpeed;
	[SerializeField] 
	private float m_StickToGroundForce;
	[SerializeField] 
	private float m_GravityMultiplier;
	[SerializeField] 
	private float m_StepInterval;

	private bool m_Jump;
	private float m_YRotation;
	private Vector2 m_Input;
	private Vector3 m_MoveDir = Vector3.zero;
	private CharacterController m_CharacterController;
	private CollisionFlags m_CollisionFlags;
	private bool m_PreviouslyGrounded;
	private float m_StepCycle;
	private float m_NextStep;
	private bool m_Jumping;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
