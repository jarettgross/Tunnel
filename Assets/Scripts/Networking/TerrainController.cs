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
	private float readyTime = 0.0f;
	private bool[] moveTime = new bool[3];

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
		readyTime = Time.time;
	}


	// Update is called once per frame
	void Update () {

//		if (ready) {
//			if (!moveTime [0] && (Time.time - readyTime) > 10) {
//				moveTime [0] = true;
//				terrainManager.MoveWallsIn ();
//			} else if (!moveTime [1] && (Time.time - readyTime) > 20) {
//				moveTime [1] = true;
//				terrainManager.MoveWallsIn ();
//			} else if (!moveTime [2] && (Time.time - readyTime) > 30) {
//				moveTime [2] = true;
//				terrainManager.MoveWallsIn ();
//			}
//		}

		if (!isLocalPlayer)
			return;

		if (!ready) {
			return;
		}

		terrainManager.UpdateWorld (gameObject.transform.position);
	}

	[Command]
	public void CmdDeform(Vector3 position, int radius) {
		Deformation deformation = new Deformation(position, Deformation.DeformationType.Cube, radius);
		networkManager.SendDeformation(deformation);
	}
		
	[ClientRpc]
	public void RpcDeform(Vector3 position, Deformation.DeformationType type, int radius) {
		if (!isLocalPlayer)
			return;

		terrainManager.Deform (new Deformation(position, type, radius));
	}
}
