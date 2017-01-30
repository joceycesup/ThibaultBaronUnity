using UnityEngine;
using System.Collections;

public class PiggyReset : MonoBehaviour {

	void OnTriggerExit (Collider coll) {
		Debug.Log (coll.gameObject + " exited");
		if (coll.tag == "Piggy") {
			coll.gameObject.GetComponent<FlyingPig> ().ResetPig ();
		}
	}
}
