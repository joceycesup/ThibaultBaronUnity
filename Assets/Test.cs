using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    
	void Start () {
	}
	
	void Update () {
        //if (Input.GetButton("Fire1"))
            //gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * 30f;

        if (Input.GetButton("Fire2"))
            gameObject.transform.position += new Vector3(0f, 0f, Mathf.Min(1f, 1.1f - transform.position.z));
        if (Input.GetButton("Fire3"))
            gameObject.transform.position += new Vector3(0f, 0f, - Mathf.Min(1f, transform.position.z));
    }

    void OnCollisionEnter (Collision coll) {
        if (coll.gameObject.tag == "Wall")
            Physics.gravity = coll.contacts[0].normal * -9.81f;
    }
}
