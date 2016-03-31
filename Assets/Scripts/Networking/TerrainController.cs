using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TerrainController : NetworkBehaviour {

	public Camera playerCamera;
	public LayerMask layerMask;
	public CustomNetworkManager networkManager; // dont initialize

	private TerrainManager terrainManager;

	private bool ready = false;

	public override void OnStartLocalPlayer() {
		layerMask = ~layerMask;
		networkManager = gameObject.GetComponent<CustomNetworkManager> ();
	}

	public void Initialize() {
		GameObject tm = GameObject.Find("Terrain Manager(Clone)");
		if (tm == null) {
			Debug.LogError ("Terrain Manager null in TerrainController DisplayWorld()");
			return;
		}
		
		Debug.Log ("Displaying World");
		terrainManager = tm.GetComponent<TerrainManager> ();
		gameObject.transform.position = new Vector3 (5, 20, 5);
		ready = true;
	}


	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer)
			return;

		if (!ready) {
			return;
		}

		terrainManager.UpdateWorld (gameObject.transform.position);
	}

	[Command]
	public void CmdDeform(Vector3 position) {
		Deformation deformation = new Deformation(position, Deformation.DeformationType.Cube, 1);
		networkManager.SendDeformation(deformation);
	}
		
	[ClientRpc]
	public void RpcDeform(Vector3 position, Deformation.DeformationType type, int radius) {
		if (!isLocalPlayer)
			return;

		terrainManager.Deform (new Deformation(position, type, radius));
	}
}
