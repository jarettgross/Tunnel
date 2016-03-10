using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TerrainUpdater : NetworkBehaviour {

	public Camera playerCamera;
	public LayerMask layerMask;

	private World world;



	// Use this for initialization
	void Start () {
		if (!isLocalPlayer)
			return;

		world = GameObject.Find ("World").GetComponent<World> ();
		layerMask = ~layerMask;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
			return;

		world.UpdateWorld (gameObject.transform.position);

		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Transform transform = playerCamera.transform;
			Ray ray = new Ray (transform.position, transform.forward); 

			Debug.Log ("Shooting ray");
			if (Physics.Raycast(ray, out hit, 100f, layerMask)) { // , LayerMask.GetMask("Player1")
				Debug.Log (hit.collider);
				Vector3 position = hit.point;
				CmdDeform (position);
			}
		}
	}

	[Command]
	private void CmdDeform(Vector3 position) {
		Deformation deformation = new Deformation(position, Deformation.DeformationType.Cube, 1);
		RpcDeform (deformation.Position, deformation.GetDeformationType(), deformation.Radius);
	}

	[ClientRpc]
	private void RpcDeform(Vector3 position, Deformation.DeformationType type, int radius) {
		world.Deform (new Deformation(position, type, radius));
	}
}
