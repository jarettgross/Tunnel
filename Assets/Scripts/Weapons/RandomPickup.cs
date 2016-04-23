using UnityEngine;
using System.Collections;

public class RandomPickup : MonoBehaviour {

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Player") {
			if (Random.Range(0, 1) < 0.5) { //give ammo and jetpack fuel to player
				collision.gameObject.GetComponent<WeaponController>().GetCurrentWeapon().currentClipSize = collision.gameObject.GetComponent<WeaponController>().GetCurrentWeapon().ClipSize;
			
				collision.gameObject.GetComponent<PlayerController> ().fuelAmount += 5;
				if (collision.gameObject.GetComponent<PlayerController> ().fuelAmount > collision.gameObject.GetComponent<PlayerController> ().originalFuelAmount) {
					collision.gameObject.GetComponent<PlayerController> ().fuelAmount = collision.gameObject.GetComponent<PlayerController> ().originalFuelAmount;
				}

			} else { //give health and jetpack fuel to player
				collision.gameObject.GetComponent<PlayerGUI>().currentHealth += 30;
				if (collision.gameObject.GetComponent<PlayerGUI>().currentHealth > collision.gameObject.GetComponent<PlayerGUI>().hitPoints) {
					collision.gameObject.GetComponent<PlayerGUI> ().currentHealth = collision.gameObject.GetComponent<PlayerGUI> ().hitPoints;
				}

				collision.gameObject.GetComponent<PlayerController> ().fuelAmount += 5;
				if (collision.gameObject.GetComponent<PlayerController> ().fuelAmount > collision.gameObject.GetComponent<PlayerController> ().originalFuelAmount) {
					collision.gameObject.GetComponent<PlayerController> ().fuelAmount = collision.gameObject.GetComponent<PlayerController> ().originalFuelAmount;
				}
			}
			collision.gameObject.GetComponent<TerrainController> ().CmdSpawnPickupBox ();
			Destroy (gameObject);
		}
	}
}
