using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson {
	[Serializable]
	public class MouseLookTP {
		public float XSensitivity = 2f;
		public float YSensitivity = 2f;
		public bool clampVerticalRotation = true;
		public float MinimumX = -90F;
		public float MaximumX = 90F;
		public bool smooth;
		public float smoothTime = 5f;
		public bool lockCursor = true;

		private Vector3 initialPos;
		public Vector3 maxPos;
		public Vector3 aimPos;
		private Quaternion m_CharacterTargetRot;
		private Quaternion m_CameraTargetRot;
		private bool m_cursorIsLocked = true;

		public Vector3 camTargetPos;
		private Vector3 tmpCamTargetPos;
		public float camSpeed = 8f;
		private bool aiming = false;

		public void Init (Transform character, Transform camera) {
			m_CharacterTargetRot = character.localRotation;
			m_CameraTargetRot = camera.parent.localRotation;
			initialPos = camera.localPosition;
			maxPos += initialPos;
			camTargetPos = Camera.main.transform.localPosition;
		}


		public void LookRotation (ThirdPersonCharacter character, Transform camera) {
			float yRot = CrossPlatformInputManager.GetAxis ("Mouse Y") * YSensitivity;
			
			m_CameraTargetRot = camera.parent.localRotation * Quaternion.Euler (-yRot, 0f, 0f);
			/*
			if (clampVerticalRotation)
				m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);//*/

			if (smooth) {/*
				character.transform.localRotation = Quaternion.Slerp (character.transform.localRotation, m_CharacterTargetRot,
					smoothTime * Time.deltaTime);/*/
				//character.Turn (yRot);
				//character.Move (Vector3.right * -yRot, false, false);
				//character.Move (Vector3.right * xRot, false, false);
				//*/
				camera.parent.localRotation = Quaternion.Slerp (camera.parent.localRotation, m_CameraTargetRot,
					smoothTime * Time.deltaTime);
			} else {/*
				character.transform.localRotation = m_CharacterTargetRot;/*/
				//character.Turn (yRot);
				//character.Move (Vector3.right * -yRot, false, false);
				//*/
				camera.parent.localRotation = m_CameraTargetRot;
			}

			UpdateCursorLock ();
		}

		public void SetCursorLock (bool value) {
			lockCursor = value;
			if (!lockCursor) {//we force unlock the cursor if the user disable the cursor locking helper
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		public void UpdateCursorLock () {
			//if the user set "lockCursor" we check & properly lock the cursos
			if (lockCursor)
				InternalLockUpdate ();
		}

		public void Zoom (Transform character, float factor) {
			if (factor != 0f && !aiming) {
				factor = Vector3.Distance (initialPos, Camera.main.transform.localPosition) / Vector3.Distance (initialPos, maxPos) - factor * 0.1f;
				camTargetPos = Vector3.Lerp (initialPos, maxPos, factor);
			}
			if (Camera.main.transform.localPosition != camTargetPos) {
				Camera.main.transform.localPosition = Vector3.Lerp (Camera.main.transform.localPosition, camTargetPos, (camSpeed * Time.deltaTime) / Vector3.Distance (Camera.main.transform.localPosition, camTargetPos));
			}
		}

		public void Aim (bool value) {
			if (aiming == (aiming = value))
				return;

			Debug.Log ("Aiming : " + value);
			if (value) {
				tmpCamTargetPos = camTargetPos;
				camTargetPos = aimPos;
			} else {
				camTargetPos = tmpCamTargetPos;
			}
		}

		private void InternalLockUpdate () {
			if (Input.GetKeyUp (KeyCode.Escape)) {
				m_cursorIsLocked = false;
			} else if (Input.GetMouseButtonUp (0)) {
				m_cursorIsLocked = true;
			}

			if (m_cursorIsLocked) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			} else if (!m_cursorIsLocked) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		Quaternion ClampRotationAroundXAxis (Quaternion q) {
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1.0f;

			float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

			angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

			q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

			return q;
		}
	}
}
