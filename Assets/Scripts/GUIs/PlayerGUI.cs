using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerGUI : NetworkBehaviour {

	//PLAYER HEALTH INFO
	private float hitPoints;

	[SyncVar]
	private float currentHealth;

	//*****************************

	void Awake() {
		hitPoints = 100;
		currentHealth = hitPoints;
	}

	//Need to initialize after awake so that player can get characterClass's healthPoints value
	public void Initialize() {
		hitPoints = gameObject.GetComponent<CharacterClass> ().HealthPoints;
		currentHealth = hitPoints;
	}

	public bool ReceiveDamage(float damageAmount) {
		currentHealth -= damageAmount;
		if (currentHealth <= 0) {
			Destroy (gameObject);
			return true;
		}
		Debug.Log("Took damage. Health now: " + currentHealth);
		return false;
	}

	public float HealthRatio() {
		return currentHealth / hitPoints;
	}
}