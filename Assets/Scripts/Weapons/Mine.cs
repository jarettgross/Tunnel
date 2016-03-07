using UnityEngine;
using System.Collections;

public class Mine : MonoBehaviour {

	private GameObject owner;

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player" && other.gameObject != owner) {
			World world = (World)ScriptableObject.FindObjectOfType<World> ();
			world.Deform (new Deformation(gameObject.transform.position, Deformation.DeformationType.Cube, 1));
			Destroy (gameObject);
		}
	}

	void Update() {
		float distToGround = gameObject.GetComponent<BoxCollider> ().bounds.extents.y;
		if (!Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f) && !Physics.Raycast(transform.position, Vector3.up, distToGround + 0.1f)) {
			DestroyImmediate (gameObject);
		}
	}

	public void setOwner(GameObject _owner) {
		owner = _owner;
	}
}
