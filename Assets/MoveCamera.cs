using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {
	public Transform origin;
	public Transform target;
	private float totalDistance;
	public float minSpeed;
	public float maxSpeed;

	void Start () {
		totalDistance = Vector3.Distance (origin.position, target.position);
		transform.position = origin.position;
	}

	void Update () {
		float d = Vector3.Distance (transform.position, target.position);
		float factor = 1 - d / totalDistance - 0.5f;
		float speed = maxSpeed - factor * factor * 4 * (maxSpeed - minSpeed);
		transform.position = Vector3.Lerp (transform.position, target.position, (speed * Time.deltaTime) / d);
	}
}
