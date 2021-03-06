﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TerrainController : NetworkBehaviour {

	public ParticleSystem hitEffect = null;
	public ParticleSystem pickupBoxEffectHealth = null;
	public ParticleSystem pickupBoxEffectAmmo = null;

	public Camera playerCamera;
	public LayerMask layerMask;
	public CustomNetworkManager networkManager; // dont initialize

	private TerrainManager terrainManager;

	private bool ready = false;
	private float readyTime = 0.0f;
	private bool[] moveTime = new bool[3];

	public GameObject pickupBox;

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
        // spawn is handled by CustomNetworkManager
		//gameObject.transform.position = new Vector3 (5, 20, 5);
		ready = true;
		readyTime = Time.time;

		CmdSpawnPickupBox ();
		CmdSpawnPickupBox ();
		CmdSpawnPickupBox ();
		CmdSpawnPickupBox ();
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

	[Command]
	public void CmdTerrainParticles(Vector3 position, Vector3 hitDirection) {
		networkManager.SendParticleInfo (position, hitDirection);
	}

	[ClientRpc]
	public void RpcTerrainParticles(Vector3 position, Vector3 hitDirection) {
		if (!isLocalPlayer)
			return;

		Instantiate (hitEffect, position, Quaternion.FromToRotation (Vector3.forward, hitDirection));
		DestroyTerrainParticles ();
	}

	private void DestroyTerrainParticles() {
		GameObject[] terrainParticleSystems = GameObject.FindGameObjectsWithTag ("TerrainParticles");
		foreach (GameObject tps in terrainParticleSystems) {
			Destroy (tps, tps.GetComponent<ParticleSystem>().startLifetime);
		}
	}

	[Command]
	public void CmdSpawnPickupBox() {
		NetworkServer.Spawn ((GameObject)Instantiate (pickupBox, new Vector3 (Random.Range (5, 75), Random.Range (2, 20), Random.Range (5, 75)), Quaternion.identity));
		networkManager.SendPickupInfo ();
	}

	[ClientRpc]
	public void RpcSpawnPickupBox(Vector3 pos) {
		if (!isLocalPlayer)
			return;
	}

	[Command]
	public void CmdDestroyPickupBox(NetworkInstanceId id) {
		networkManager.SendDestroyPickupInfo (id);
	}

	[ClientRpc]
	public void RpcDestroyPickupBox(NetworkInstanceId id) {
		GameObject[] pickupBoxes = GameObject.FindGameObjectsWithTag ("PickupBox");
		foreach (GameObject pb in pickupBoxes) {
			if (pb.GetComponent<NetworkIdentity>().netId == id) {
				Destroy (pb);
			}
		}
	}

	[Command]
	public void CmdPickupBoxParticles(Vector3 pos, Quaternion dir, bool isHealthUpgrade) {
		networkManager.SendPickupBoxParticleInfo (pos, dir, isHealthUpgrade);
	}

	[ClientRpc]
	public void RpcPickupBoxParticles(Vector3 pos, Quaternion dir, bool isHealthUpgrade) {
		if (!isLocalPlayer)
			return;

		if (isHealthUpgrade) {
			Instantiate (pickupBoxEffectHealth, pos, Quaternion.Euler(0, 90, 0));
		} else {
			Instantiate (pickupBoxEffectAmmo, pos, Quaternion.Euler(0, 90, 0));
		}
		DestroyPickupBoxParticles ();
	}

	private void DestroyPickupBoxParticles() {
		GameObject[] pickupBoxParticleSystems = GameObject.FindGameObjectsWithTag ("PickupBoxParticles");
		foreach (GameObject pps in pickupBoxParticleSystems) {
			Destroy (pps, pps.GetComponent<ParticleSystem>().startLifetime);
		}
	}
}
