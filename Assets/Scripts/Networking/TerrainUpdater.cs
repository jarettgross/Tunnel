using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TerrainUpdater : NetworkBehaviour {

	private World world;
	private Camera playerCamera;

	// Use this for initialization
	void Start () {
		world = GameObject.Find ("World").GetComponent<World> ();
		playerCamera = GetComponent<SimpleSmoothMouseLook> ().cameras [0];
	}
	
	// Update is called once per frame
	void Update () {
		world.UpdateWorld (gameObject.transform.position);

		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Transform transform = playerCamera.transform;
			Ray ray = new Ray (transform.position, transform.forward); 

			if (Physics.Raycast(ray, out hit)) {
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
