using UnityEngine;
using UnityEngine.Networking;

public class RandomPickup : NetworkBehaviour {

	private NetworkInstanceId uniqueBoxId;

	void Start() {
		uniqueBoxId = GetComponent<NetworkIdentity> ().netId;
	}

	void OnTriggerEnter(Collider collision) {
		bool isHealthUpgrade;
		if (collision.gameObject.tag == "Player") {
			if (Random.Range(0, 2) == 0) { //give ammo and jetpack fuel to player
				foreach (WeaponBase weapon in collision.gameObject.GetComponent<WeaponController>().weapons) {
					weapon.currentClipSize = weapon.ClipSize;
				}
				isHealthUpgrade = false;
			} else { //give health and jetpack fuel to player
				collision.gameObject.GetComponent<PlayerGUI>().currentHealth += 30;
				if (collision.gameObject.GetComponent<PlayerGUI>().currentHealth > collision.gameObject.GetComponent<PlayerGUI>().hitPoints) {
					collision.gameObject.GetComponent<PlayerGUI> ().currentHealth = collision.gameObject.GetComponent<PlayerGUI> ().hitPoints;
				}
				isHealthUpgrade = true;
			}

			collision.gameObject.GetComponent<PlayerController> ().fuelAmount += 5;
			if (collision.gameObject.GetComponent<PlayerController> ().fuelAmount > collision.gameObject.GetComponent<PlayerController> ().originalFuelAmount) {
				collision.gameObject.GetComponent<PlayerController> ().fuelAmount = collision.gameObject.GetComponent<PlayerController> ().originalFuelAmount;
			}

			if (collision.gameObject.GetComponent<CharacterClass>().className == "Stealth") {
				collision.gameObject.GetComponent<PlayerController> ().invisibilityRemaining += 5.0f;
				if (collision.gameObject.GetComponent<PlayerController> ().invisibilityRemaining > 20.0f) {
					collision.gameObject.GetComponent<PlayerController> ().invisibilityRemaining = 20.0f;
				}
			}
				
			collision.gameObject.GetComponent<TerrainController> ().CmdSpawnPickupBox ();
			collision.gameObject.GetComponent<TerrainController> ().CmdDestroyPickupBox (uniqueBoxId);

			collision.gameObject.GetComponent<TerrainController> ().CmdPickupBoxParticles (transform.position, transform.rotation, isHealthUpgrade);
		}
	}
}
