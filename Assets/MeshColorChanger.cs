using UnityEngine;
using System.Collections;

public class MeshColorChanger : MonoBehaviour {
	public Color color;
	
	void Awake () {
		gameObject.GetComponent<MeshRenderer> ().material.color = color;
		Destroy (this);
	}
}
