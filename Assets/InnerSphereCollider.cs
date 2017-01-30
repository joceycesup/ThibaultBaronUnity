using UnityEngine;
using System.Collections;

public class InnerSphereCollider : MonoBehaviour {
	public GameObject sphere;
	public GameObject player;

	private float radius;

	void Start () {
		radius = sphere.GetComponent<SphereCollider> ().radius * sphere.transform.localScale.z;
		transform.GetChild (0).localPosition = new Vector3 (0f, 0f, radius + transform.GetChild (0).gameObject.GetComponent<BoxCollider> ().bounds.extents.z);
	}

	void Update () {
		transform.rotation = Quaternion.LookRotation (player.transform.position);
	}
}
