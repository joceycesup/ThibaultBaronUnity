using UnityEngine;
using System.Collections;

public class FlyingPig : MonoBehaviour {
	private Vector3 origin;
	public Vector3 direction;
	public float minSpeed;
	public float maxSpeed;
	public Vector3 rotation;
	private float speed;

	private static float maxRotation = 180f;

	void Start () {
		origin = transform.position;
		speed = Random.Range (minSpeed, maxSpeed);
		rotation = new Vector3 (Random.Range (-maxRotation, maxRotation), Random.Range (-maxRotation, maxRotation), Random.Range (-maxRotation, maxRotation));
	}

	void Update () {
		transform.Rotate (rotation * Time.deltaTime);
		transform.position += Vector3.ClampMagnitude (direction, speed * Time.deltaTime);
	}

	public void ResetPig () {
		transform.position = origin;
	}
}
