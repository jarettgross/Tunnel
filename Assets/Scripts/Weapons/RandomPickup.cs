using UnityEngine;
using System.Collections;

public class RandomPickup : MonoBehaviour {

	void OnCollisionEnter(Collision collision) {
		Debug.Log ("collsiion entered");
		if (collision.gameObject.tag == "Player") {
			Debug.Log ("giving bonus");
			if (Random.Range(0, 1) < 0.5) { //give ammo to player
				collision.gameObject.GetComponent<WeaponController>().GetCurrentWeapon().currentClipSize = collision.gameObject.GetComponent<WeaponController>().GetCurrentWeapon().ClipSize;
			} else { //give health to player
				collision.gameObject.GetComponent<PlayerGUI>().currentHealth += 30;
			}
			collision.gameObject.GetComponent<TerrainController> ().CmdSpawnPickupBox ();
			Destroy (gameObject);
		}
	}
}
