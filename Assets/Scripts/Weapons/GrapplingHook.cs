using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour {

	public int speed;

	private bool inAir = true;

	public void Start() {
		GetComponent<Rigidbody> ().AddForce (gameObject.transform.forward * speed);
	}

	void OnCollisionEnter (Collision col) {
		
		if (col.gameObject.name == "chunk") {
			inAir = false;

			FixedJoint joint = gameObject.AddComponent<FixedJoint> ();
			joint.anchor = col.transform.position;
		}
	}

	public bool InAir {
		get { 
			return inAir;
		}
	}
}
