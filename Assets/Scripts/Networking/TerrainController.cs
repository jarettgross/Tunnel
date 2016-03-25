using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TerrainController : NetworkBehaviour {

	public Camera playerCamera;
	public LayerMask layerMask;
	public GameObject terrainManagerPrefab;
	public CustomNetworkManager networkManager; // dont initialize

	private TerrainManager terrainManager;


	public override void OnStartLocalPlayer() {
		layerMask = ~layerMask;
		networkManager = gameObject.GetComponent<CustomNetworkManager> ();


	}


	private void DisplayWorld() {
		terrainManager = ((GameObject)Instantiate (terrainManagerPrefab)).GetComponent<TerrainManager> ();
	}


	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
			return;

		terrainManager.UpdateWorld (gameObject.transform.position);

		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Transform transform = playerCamera.transform;
			Ray ray = new Ray (transform.position, transform.forward); 

			if (Physics.Raycast(ray, out hit, 100f, layerMask)) {
				Debug.Log (hit.collider);
				Vector3 position = hit.point;
				CmdDeform (position);
				//terrainManager.Deform (position);			
			}
		}
	}

	[Command]
	private void CmdDeform(Vector3 position) {
		Debug.Log ("In Cmd");
		Deformation deformation = new Deformation(position, Deformation.DeformationType.Cube, 1);
//		RpcDeform (deformation.Position, deformation.GetDeformationType(), deformation.Radius);
		networkManager.SendDeformation(deformation);
	}
		
	[ClientRpc]
	public void RpcDeform(Vector3 position, Deformation.DeformationType type, int radius) {
		if (!isLocalPlayer)
			return;

		Debug.Log ("Received deformation");
		terrainManager.Deform (new Deformation(position, type, radius));
	}
}
