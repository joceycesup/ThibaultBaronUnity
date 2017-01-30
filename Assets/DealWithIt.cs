using UnityEngine;
using System.Collections;

public class DealWithIt : MonoBehaviour {
	public GameObject glasses;

	void Update () {
		if (Input.GetButtonDown ("Deal")) {
			glasses.SetActive (!glasses.activeInHierarchy);
		}
	}
}
