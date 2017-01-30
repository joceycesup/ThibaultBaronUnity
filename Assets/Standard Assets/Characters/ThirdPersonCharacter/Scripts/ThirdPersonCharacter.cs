using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson {
	[RequireComponent (typeof (Rigidbody))]
	[RequireComponent (typeof (CapsuleCollider))]
	[RequireComponent (typeof (Animator))]
	public class ThirdPersonCharacter : MonoBehaviour {
		[SerializeField]
		float m_MovingTurnSpeed = 360;
		[SerializeField]
		float m_StationaryTurnSpeed = 180;
		[SerializeField]
		float m_JumpPower = 12f;
		[Range (1f, 4f)]
		[SerializeField]
		float m_GravityMultiplier = 2f;
		[SerializeField]
		float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField]
		float m_MoveSpeedMultiplier = 1f;
		[SerializeField]
		float m_AnimSpeedMultiplier = 1f;
		[SerializeField]
		public float m_GroundCheckDistance = 0.2f;//0.1f
		[SerializeField]
		public float m_MinDotProductGround = 0.97f;//5 deg
		[SerializeField]
		public float m_MaxRotateRate = 60f;
		[SerializeField]
		public float m_GrapplingSpeed = 3f;

		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching;

		Vector3 gravity;
		RaycastHit target;

		void Awake () {
			gravity = Physics.gravity;
		}

		void Start () {
			m_Animator = GetComponent<Animator> ();
			m_Rigidbody = GetComponent<Rigidbody> ();
			m_Capsule = GetComponent<CapsuleCollider> ();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;
		}

		public void Move (Vector3 move, bool crouch, bool jump) {
			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f)
				move.Normalize ();
			move = transform.InverseTransformDirection (move);
			//Debug.Log("inverse : " + move);
			CheckGroundStatus ();
			move = Vector3.ProjectOnPlane (move, Quaternion.Inverse (transform.rotation) * m_GroundNormal);
			//move = Vector3.ProjectOnPlane(move, m_GroundNormal);
			m_TurnAmount = Mathf.Atan2 (move.x, move.z);
			m_ForwardAmount = move.z;

			ApplyExtraTurnRotation ();

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded) {
				HandleGroundedMovement (crouch, jump);
			} else {
				HandleAirborneMovement ();
			}

			ScaleCapsuleForCrouching (crouch);
			PreventStandingInLowHeadroom ();

			// send input and other state parameters to the animator
			UpdateAnimator (move);
		}

		void ScaleCapsuleForCrouching (bool crouch) {
			if (m_IsGrounded && crouch) {
				if (m_Crouching)
					return;
				m_Capsule.height = m_Capsule.height / 2f;
				m_Capsule.center = m_Capsule.center / 2f;
				m_Crouching = true;
			} else {
				Ray crouchRay = new Ray (m_Rigidbody.position + transform.up * m_Capsule.radius * k_Half, transform.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast (crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
					m_Crouching = true;
					return;
				}
				m_Capsule.height = m_CapsuleHeight;
				m_Capsule.center = m_CapsuleCenter;
				m_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom () {
			// prevent standing up in crouch-only zones
			if (!m_Crouching) {
				Ray crouchRay = new Ray (m_Rigidbody.position + transform.up * m_Capsule.radius * k_Half, transform.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast (crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
					m_Crouching = true;
				}
			}
		}

		public void Turn (float turnAmount) {
			m_TurnAmount = turnAmount;
			m_Animator.SetFloat ("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
		}

		void UpdateAnimator (Vector3 move) {
			// update the animator parameters
			m_Animator.SetFloat ("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
			m_Animator.SetFloat ("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			m_Animator.SetBool ("Crouch", m_Crouching);
			m_Animator.SetBool ("OnGround", m_IsGrounded);
			if (!m_IsGrounded) {
				m_Animator.SetFloat ("Jump", (Quaternion.Inverse (transform.rotation) * m_Rigidbody.velocity).y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat (
					m_Animator.GetCurrentAnimatorStateInfo (0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
			if (m_IsGrounded) {
				m_Animator.SetFloat ("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0) {
				m_Animator.speed = m_AnimSpeedMultiplier;
			} else {
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}

		void HandleAirborneMovement () {
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (gravity * m_GravityMultiplier) - gravity;
			m_Rigidbody.AddForce (extraGravityForce);

			m_GroundCheckDistance = (Vector3.Dot (m_Rigidbody.velocity, transform.up) < 0 && (target.collider == null || Vector3.Dot (target.normal, transform.up) > 0)) ? m_OrigGroundCheckDistance : 0.01f;
		}

		void HandleGroundedMovement (bool crouch, bool jump) {
			// check whether conditions are right to allow a jump:
			if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo (0).IsName ("Grounded")) {
				// jump!
				//m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
				m_Rigidbody.velocity += transform.up * m_JumpPower;
				m_IsGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
			}
		}

		void ApplyExtraTurnRotation () {
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp (m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate (0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}
		
		public void OnAnimatorMove () {
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (m_IsGrounded && Time.deltaTime > 0) {
				Vector3 v = Quaternion.Inverse (transform.rotation) * (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = (Quaternion.Inverse (transform.rotation) * m_Rigidbody.velocity).y;
				m_Rigidbody.velocity = transform.rotation * v;
			}
		}
		
		void CheckGroundStatus () {
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine (transform.position + transform.up * 0.1f, transform.position + transform.up * 0.1f + transform.rotation * (Vector3.down * m_GroundCheckDistance), Color.red);
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character

			if (Physics.Raycast (transform.position + transform.up * 0.1f, transform.rotation * (Vector3.down), out hitInfo, m_GroundCheckDistance)) {
				m_GroundNormal = hitInfo.normal;
				if (Vector3.Dot (hitInfo.normal, transform.up) < m_MinDotProductGround) {
					SetGravity (hitInfo.normal);
				}
				m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
			} else {
				m_IsGrounded = false;
				m_GroundNormal = transform.up;
				m_Animator.applyRootMotion = false;
			}
		}

		public void SetGravity (Vector3 normal) {
			gravity = normal * -9.81f;
		}

		public void SetGravityTarget (Vector3 target) {
			gravity = Vector3.Normalize(transform.position - target) * -9.81f;
		}

		public bool SetTarget (RaycastHit target) {/*
			if (Vector3.Dot (target.normal, transform.up) >= m_MinDotProductGround)
				return false;//*/
			this.target = target;
			SetGravityTarget (target.point);
			HandleAirborneMovement ();
			m_Rigidbody.velocity = Vector3.zero;
			return true;
		}

		void Update () {
			//Debug.Log (gravity);
			if (target.collider != null) {
				//Debug.Log (Time.time + " grappling!");
				float factor = m_GrapplingSpeed * Time.deltaTime / Vector3.Distance (target.point, transform.position);
				transform.position = Vector3.Lerp (transform.position, target.point, factor);
				
				Vector3 targetNorm = Vector3.RotateTowards (transform.up, target.normal, (Vector3.Angle (transform.up, target.normal) * factor) * Mathf.Deg2Rad, 0f);
				//Debug.Log (m_MaxRotateRate * Time.deltaTime * Mathf.Deg2Rad);
				Quaternion gravityRot = Quaternion.LookRotation (-Vector3.Cross (targetNorm, transform.right), targetNorm);
				transform.rotation = gravityRot;

				if (Vector3.Distance (target.point, transform.position) < m_OrigGroundCheckDistance) {
					m_GroundCheckDistance = m_OrigGroundCheckDistance;
					SetGravity (target.normal);
					target = new RaycastHit ();
				}
			} else {
				Vector3 normGrav = Vector3.Normalize (-gravity);
				float dot = Vector3.Dot (normGrav, transform.up);
				if (dot < m_MinDotProductGround) {
					//Debug.Log ("changing rotation " + dot.ToString ("F3") + " ; " + Vector3.Angle (transform.up, normGrav));
					//Vector3 targetNorm = Vector3.Lerp (transform.up, normGrav, m_MaxRotateRate * Time.deltaTime / Vector3.Angle (transform.up, normGrav));
					Vector3 targetNorm = Vector3.RotateTowards (transform.up, normGrav, m_MaxRotateRate * Time.deltaTime * Mathf.Deg2Rad, 0f);
					Quaternion gravityRot = Quaternion.LookRotation (-Vector3.Cross (targetNorm, transform.right), targetNorm);
					transform.rotation = gravityRot;
					//transform.rotation = Quaternion.AngleAxis(Mathf.Min(Vector3.Angle(transform.up, m_GroundNormal), m_MaxRotateRate * Time.deltaTime), transform.right);
				}
			}
		}

		void FixedUpdate () {
			m_Rigidbody.AddForce (gravity * m_Rigidbody.mass);
		}
	}
}
