using UnityEngine;
using System.Collections;

public class Mine : MonoBehaviour {

	public GameObject owner; //player who owns this mine; can't explode on owner
	private bool isFalling;
	private bool ignoreCollision; //ignore the collision when mine is first instantiated
	public float damage;

	//Triggered by a sphere collider, slightly larger than the mine
	//Used for a player entering the mine's radius
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player" && other.gameObject != owner) {
			World world = (World)ScriptableObject.FindObjectOfType<World> ();
			world.Deform (new Deformation(gameObject.transform.position, Deformation.DeformationType.Cube, 1));
			//Add player damage
			Destroy (gameObject);
		}
	}

	//Triggered by a box collider, about the same size as the mine
	//Used for collision with terrain when the mine is falling
	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag != "Player" && isFalling && !ignoreCollision) {
			Debug.Log ("collision: " + collision.gameObject);
			ContactPoint contact = collision.contacts [0];
			Vector3 pos = contact.point;
			World world = (World)ScriptableObject.FindObjectOfType<World> ();
			world.Deform (new Deformation (pos, Deformation.DeformationType.Cube, 1));
			Destroy (gameObject);
		}
	}

	void Start() {
		isFalling = false;
		ignoreCollision = true;
		foreach (Collider col in gameObject.GetComponents<Collider>()) {
			foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
				Physics.IgnoreCollision (col, player.GetComponent<Collider> ());
			}
		}
		gameObject.GetComponent<Rigidbody> ().velocity = new Vector3 (0, -0.01f, 0);
	}

	void Update() {
		if (gameObject.GetComponent<Rigidbody>().velocity.y < 0) {
			isFalling = true;
			gameObject.GetComponent<Rigidbody> ().freezeRotation = false;
		} else {
			isFalling = false;
			ignoreCollision = false;
		}
	}
}
