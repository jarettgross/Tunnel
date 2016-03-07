using UnityEngine;
using System.Collections;

public class RandomPickup : MonoBehaviour {

	// Use this for initialization
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			other.gameObject.GetComponent<FPSWalkerEnhanced> ().setMineCounter (1);
			Destroy (gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
