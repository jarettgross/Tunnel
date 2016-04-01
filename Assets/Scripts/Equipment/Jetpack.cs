using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Jetpack : NetworkBehaviour {

	private float fuelAmount;
	public GameObject owner;

	// Use this for initialization
	void Start () {
		fuelAmount = 60.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (isLocalPlayer) {
			if (Input.GetKey (KeyCode.Space)) {
				useFuel ();	
			}
		}
	}

	private void useFuel() {
		owner.GetComponent<Rigidbody> ().AddForce (new Vector3 (0, 4, 0));
		fuelAmount -= Time.deltaTime;
	}
}
