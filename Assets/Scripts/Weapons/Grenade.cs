using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour {

	public GameObject owner;

	public float blastRadius;
	public float damage;
	public float throwForce;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = gameObject.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void throwGrenade() {
		Vector3 directionFacing = owner.transform.forward;
		Vector3 forceVector = directionFacing * throwForce;
		rb.AddForce (forceVector);
	}

	private void grenadeExplosion() {
		Collider[] objectsInExplosion = Physics.OverlapSphere (gameObject.transform.position, blastRadius);
		//Check if grenade explosion hit player
		foreach (Collider col in objectsInExplosion) {
			if (col.gameObject.tag == "Player") {
				RaycastHit hit;
				if (Physics.Raycast(gameObject.transform.position, col.gameObject.transform.position - gameObject.transform.position, out hit)) {
					if (hit.collider == col) {
						float distanceToBlast = (gameObject.transform.position - col.gameObject.transform.position).magnitude;
						float damageDropoff = 1 - distanceToBlast / blastRadius; //linear dropoff
						//Add damage to player
					}
				}
			}
		}

		//Check for collision with mesh in specific directions
		RaycastHit hitMesh;
		if (Physics.Raycast (gameObject.transform.position, Vector3.up, out hitMesh)) {
			if (hitMesh.distance <= blastRadius) {
				//Send deformation
			}
		}
		if (Physics.Raycast (gameObject.transform.position, -Vector3.up, out hitMesh)) {
			if (hitMesh.distance <= blastRadius) {
				//Send deformation
			}
		}
		if (Physics.Raycast (gameObject.transform.position, Vector3.forward, out hitMesh)) {
			if (hitMesh.distance <= blastRadius) {
				//Send deformation
			}
		}
		if (Physics.Raycast (gameObject.transform.position, -Vector3.forward, out hitMesh)) {
			if (hitMesh.distance <= blastRadius) {
				//Send deformation
			}
		}
		if (Physics.Raycast (gameObject.transform.position, Vector3.right, out hitMesh)) {
			if (hitMesh.distance <= blastRadius) {
				//Send deformation
			}
		}
		if (Physics.Raycast (gameObject.transform.position, -Vector3.right, out hitMesh)) {
			if (hitMesh.distance <= blastRadius) {
				//Send deformation
			}
		}
	}
}
