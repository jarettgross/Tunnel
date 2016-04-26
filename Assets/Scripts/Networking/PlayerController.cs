using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;
using Random = UnityEngine.Random;

[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof(SoundController))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten = 0.0f;
	[SerializeField] private float m_JumpSpeed = 0.0f;
	[SerializeField] private float m_StickToGroundForce = 0.0f;
	[SerializeField] private float m_GravityMultiplier = 0.0f;
	[SerializeField] private float m_StepInterval = 0.0f;
	[SerializeField] private AudioClip[] m_FootstepSounds = null;    // an array of footstep sounds that will be randomly selected from.
	[SerializeField] private AudioClip m_JumpSound = null;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound = null;           // the sound played when character touches back on ground.

    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private SoundController m_SoundController;


	//** Jetpack
	private bool isUsingFuel = false;
	public float originalFuelAmount = 10f;
	public float fuelAmount = 10f;
	//**

	//** Invisibility
	public float invisibilityRemaining = 20.0f; //invisiblity remaining in seconds
	public bool isInvisible = false;
	public float invisibleCooldown = -0.5f;
	public bool isCooldown = false;
	//**

	[System.NonSerialized] public NetworkInstanceId playerUniqueID; //DO NOT SET

	[SerializeField] private ParticleSystem hitEffect = null;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle/2f;
        m_Jumping = false;
        m_SoundController = GetComponent<SoundController>();
    }

	public void Initialize() {
		CharacterClass character = GetComponent<CharacterClass>();
		m_WalkSpeed = character.WalkSpeed;
		m_RunSpeed = character.RunSpeed;

		if (character.className == "Explosives") {
			gameObject.GetComponent<Renderer> ().material.color = new Color (255, 0, 0);
		} else if (character.className == "Stealth") {
			gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0);
		} else if (character.className == "Traps") {
			gameObject.GetComponent<Renderer> ().material.color = new Color (0, 255, 255);
		}

		playerUniqueID = GetComponent<NetworkIdentity> ().netId;

		Color pColor = gameObject.GetComponent<Renderer> ().material.color;
		GetComponent<ExtraWeaponController> ().CmdPlayerColor (pColor.r, pColor.g, pColor.b, playerUniqueID);
	}

	
    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

		if (!isUsingFuel && !m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
		if (!isUsingFuel && !m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

		//Handle player invisibility
		if (GetComponent<CharacterClass> ().className == "Stealth") {
			if (invisibilityRemaining > 0.0f && Input.GetKeyDown (KeyCode.Q)) {
				if (invisibleCooldown < 0.1f) {

					isInvisible = !isInvisible;
					if (isInvisible) {
						GetComponent<ExtraWeaponController> ().CmdInvisiblity (playerUniqueID, true);
					} else {
						GetComponent<ExtraWeaponController> ().CmdInvisiblity (playerUniqueID, false);
						invisibleCooldown = 5.0f;
						isCooldown = true;
					}
				}
			}

			if (isCooldown) { 
				invisibleCooldown -= Time.deltaTime;
				if (invisibleCooldown < 0.1f) {
					isCooldown = false;
				}
			}

			if (isInvisible) {
				invisibilityRemaining -= Time.deltaTime;
			}

			if (invisibilityRemaining <= 0.0f) {
				isInvisible = false;
				GetComponent<ExtraWeaponController> ().CmdInvisiblity (playerUniqueID, false);
			}
		}
	}

    private void PlayLandingSound()
    {
        m_SoundController.PlayClip(m_LandSound);
        m_NextStep = m_StepCycle + .5f;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height/2f, ~0, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x*speed;
        m_MoveDir.z = desiredMove.z*speed;

		if (Input.GetKeyDown (KeyCode.E) && fuelAmount > 0.0f) {
			isUsingFuel = !isUsingFuel;
			if (isUsingFuel) {
				GetComponent<ExtraWeaponController> ().CmdJetpackParticles (playerUniqueID);
				m_MoveDir.y = m_JumpSpeed / 2;
			} else {
				GetComponent<ExtraWeaponController> ().CmdEndJetpackParticles (playerUniqueID);
			}
		}

		if (isUsingFuel) {
			fuelAmount -= Time.fixedDeltaTime;
			if (fuelAmount <= 0.0f) {
				isUsingFuel = false;
				GetComponent<ExtraWeaponController> ().CmdEndJetpackParticles (playerUniqueID);
			}
		}

        if (!isUsingFuel && m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (!isUsingFuel && m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
		else if(!isUsingFuel)
        {
            m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

        ProgressStepCycle(speed);
    }

    private void PlayJumpSound()
    {
        m_SoundController.PlayClip(m_JumpSound);
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                            Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        AudioClip stepSound = m_FootstepSounds[n];
        m_SoundController.PlayClip(stepSound);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = stepSound;
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        //bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    }

	public void PlayHitEffect(Vector3 hitDirection) {
		hitEffect.startColor = gameObject.GetComponent<Renderer> ().material.color;
		Destroy (Instantiate (hitEffect, gameObject.transform.position, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, hitEffect.startLifetime);
	}
}