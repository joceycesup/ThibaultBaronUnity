using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson {
	[RequireComponent (typeof (ThirdPersonCharacter))]
	public class ThirdPersonUserControl : MonoBehaviour {
		private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
		private Transform m_Cam;                  // A reference to the main camera in the scenes transform
		private Vector3 m_CamForward;             // The current forward direction of the camera
		public MouseLookTP m_MouseLook = new MouseLookTP ();
		private Vector3 m_Move;
		private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

		private void Start () {
			// get the transform of the main camera
			if (Camera.main != null) {
				m_Cam = Camera.main.transform;
			} else {
				Debug.LogWarning (
					"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
				// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
			}

			// get the third person character ( this should never be null due to require component )
			m_Character = GetComponent<ThirdPersonCharacter> ();
			m_MouseLook.Init (transform, m_Cam);
		}

		private void Update () {
			RotateView ();
			if (!m_Jump) {
				m_Jump = CrossPlatformInputManager.GetButtonDown ("Jump");
			}
			if (Input.GetMouseButton (0)) {
				m_MouseLook.Aim (true);
				RaycastHit vHit = new RaycastHit ();
				//Ray vRay = Camera.main.ScreenPointToRay (Input.mousePosition);
				Ray vRay = Camera.main.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2));
				//vRay.origin = Vector3.ProjectOnPlane(vRay.origin, transform.forward);
				Vector3 front = transform.position + GetComponent<CapsuleCollider> ().radius * transform.forward + GetComponent<CapsuleCollider> ().height * transform.up / 2f;
				if (Physics.Raycast (vRay, out vHit, 1000, 1 << 8)) {
					bool enableLine = Vector3.Dot (transform.forward, vHit.point - front) > 0;
					if (enableLine) {
						GetComponent<LineRenderer> ().SetPosition (0, front);
						GetComponent<LineRenderer> ().SetPosition (1, vHit.point);
						GetComponent<LineRenderer> ().enabled = enabled;
					}
				} else {
					GetComponent<LineRenderer> ().enabled = false;
				}
			} else if (Input.GetMouseButtonUp (0)) {
				m_MouseLook.Aim (false);
				RaycastHit vHit = new RaycastHit ();
				Ray vRay = Camera.main.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2));
				//vRay.origin = Vector3.ProjectOnPlane(vRay.origin, transform.forward);
				Vector3 front = transform.position + GetComponent<CapsuleCollider> ().radius * transform.forward + GetComponent<CapsuleCollider> ().height * transform.up / 2f;
				if (Physics.Raycast (vRay, out vHit, 1000, 1 << 8)) {
					if (Vector3.Dot (transform.forward, vHit.point - front) > 0) {
						m_Jump = m_Character.SetTarget (vHit);
						GetComponent<LineRenderer> ().enabled = false;
					}
				}
				GetComponent<LineRenderer> ().enabled = false;
			}
			m_MouseLook.Zoom (transform, Input.mouseScrollDelta.y);
			/*
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("euler = "+ Quaternion.LookRotation(transform.forward, transform.up).eulerAngles);
            }//*/
		}


		// Fixed update is called in sync with physics
		private void FixedUpdate () {
			// read inputs;
			//*
			float h = Mathf.Clamp (CrossPlatformInputManager.GetAxis ("Mouse X") * m_MouseLook.XSensitivity, -1f, 1f);/*/
			float h = CrossPlatformInputManager.GetAxis ("Horizontal");//*/
			//Debug.Log (h);
			float v = CrossPlatformInputManager.GetAxis ("Vertical");
			bool crouch = Input.GetKey (KeyCode.C);

			m_Move = v * transform.forward + h * transform.right;
#if !MOBILE_INPUT
			// walk speed multiplier
			if (Input.GetKey (KeyCode.LeftShift))
				m_Move *= 0.5f;
#endif

			// pass all parameters to the character control script
			m_Character.Move (m_Move, crouch, m_Jump);
			m_Jump = false;
			m_MouseLook.UpdateCursorLock ();
		}

		private void RotateView () {
			m_MouseLook.LookRotation (m_Character, m_Cam);
		}
	}
}
