using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ExtraWeaponController : NetworkBehaviour {

	public ParticleSystem grenadeExplosion;

	public CustomNetworkManager networkManager; // dont initialize

	public override void OnStartLocalPlayer() {
		networkManager = gameObject.GetComponent<CustomNetworkManager> ();
	}


	//GRENADE PARTICLES

	[Command]
	public void CmdGrenadeParticles(Vector3 pos, Vector3 dir) {
		networkManager.SendGrenadeParticleInfo (pos, dir);
	}

	[ClientRpc]
	public void RpcGrenadeParticles(Vector3 pos, Vector3 dir) {
		if (!isLocalPlayer)
			return;

		Instantiate (grenadeExplosion, pos, Quaternion.FromToRotation(Vector3.forward, dir));
		DestroyGrenadeParticles ();
	}

	private void DestroyGrenadeParticles() {
		GameObject[] grenadeParticleSystems = GameObject.FindGameObjectsWithTag ("GrenadeParticles");
		foreach (GameObject gps in grenadeParticleSystems) {
			Destroy (gps, gps.GetComponent<ParticleSystem>().startLifetime);
		}
	}


	//JETPACK PARTICLES

	[Command]
	public void CmdJetpackParticles(int id) {
		networkManager.SendJetpackParticleID (id);
	}

	[ClientRpc]
	public void RpcJetpackParticles(int id) {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject player in players) {
			if (player.GetComponent<PlayerController>().playerUniqueID == id) {
				player.transform.FindChild ("JetpackTrail").gameObject.GetComponent<ParticleSystem>().Play();
			}
		}
	}

	[Command]
	public void CmdEndJetpackParticles(int id) {
		networkManager.SendEndJetpackParticleID (id);
	}

	[ClientRpc]
	public void RpcEndJetpackParticles(int id) {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject player in players) {
			if (player.GetComponent<PlayerController>().playerUniqueID == id) {
				player.transform.FindChild ("JetpackTrail").gameObject.GetComponent<ParticleSystem>().Stop();
			}
		}
	}
}
