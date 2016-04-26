using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

// FIXME this should be renamed to HealthController or something

public class PlayerGUI : NetworkBehaviour {

	//PLAYER HEALTH INFO
	public float hitPoints;

	[SyncVar]
	public float currentHealth;

    // player death stuff
	public int numDeaths = 0;
    private float deathTime;
    private bool isDead;
    private const float RESPAWN_TIME = 5.0f;
    [SyncVar]
    private int spawnIndex;

    private const int NUM_SPAWNS = 4;
    private Vector3[] spawns;

	//*****************************

	void Awake() {
		hitPoints = 100;
		currentHealth = hitPoints;
        isDead = false;
        // FIXME better spawns
        // FIXME duplicated code in CustomNetworkManager
        spawns = new Vector3[NUM_SPAWNS];
        spawns[0] = new Vector3(5, 20, 5);
        spawns[1] = new Vector3(75, 20, 75);
        spawns[2] = new Vector3(5, 20, 75);
        spawns[3] = new Vector3(75, 20, 5);
        spawnIndex = 0;
    }

	//Need to initialize after awake so that player can get characterClass's healthPoints value
	public void Initialize() {
		hitPoints = gameObject.GetComponent<CharacterClass> ().HealthPoints;
		currentHealth = hitPoints;
    }

	public bool ReceiveDamage(float damageAmount) {
		currentHealth -= damageAmount;
		if (currentHealth <= 0) {
            PlayerDied();
			return true;
		}
		Debug.Log("Took damage. Health now: " + currentHealth);
		return false;
	}

	public float HealthRatio() {
		return currentHealth / hitPoints;
	}

    private void PlayerDied()
    {
        deathTime = Time.time;
        isDead = true;
        Debug.Log("Player died: " + deathTime);
        RpcPlayerDied();
    }

    [ClientRpc]
    private void RpcPlayerDied()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        gameObject.GetComponent<CharacterController>().enabled = false;
        if (isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
            gameObject.GetComponent<SimpleSmoothMouseLook>().enabled = false;
            gameObject.GetComponent<ActionController>().enabled = false;
        }
        foreach (Transform child in transform)
        {
            foreach (Transform child2 in child)
            {
                child2.gameObject.SetActive(false);
            }
        }
    }

    private void PlayerRespawn()
    {
        currentHealth = hitPoints;
        isDead = false;
        Debug.Log("Respawning Player (server): " + Time.time);
        spawnIndex = Random.Range(0, 2);
        RpcPlayerRespawn();
    }

    [ClientRpc]
    private void RpcPlayerRespawn()
    {
        Debug.Log("Respawning Player (client)");
		GetComponent<PlayerController> ().fuelAmount = GetComponent<PlayerController> ().originalFuelAmount;
		foreach (WeaponBase weapon in GetComponent<WeaponController> ().weapons) {
			weapon.currentClipSize = weapon.ClipSize;
		}
		if (GetComponent<CharacterClass>().className == "Stealth") {
			GetComponent<PlayerController> ().invisibilityRemaining = 20.0f;
		}

        gameObject.transform.position = spawns[spawnIndex];
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<CapsuleCollider>().enabled = true;
        gameObject.GetComponent<CharacterController>().enabled = true;
        if (isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = true;
            gameObject.GetComponent<SimpleSmoothMouseLook>().enabled = true;
            gameObject.GetComponent<ActionController>().enabled = true;
        }
        foreach (Transform child in transform)
        {
            foreach (Transform child2 in child)
            {
                child2.gameObject.SetActive(true);
            }
        }
    }

	[ClientRpc]
	private void RpcDisplayDeathText(string text) {
		if (GetComponent<HealthBar> ().deathText != null) {
			GetComponent<HealthBar> ().deathText.text = text;
		}
	}

	[ClientRpc]
	private void RpcDisplayLivesCount(string text) {
		if (GetComponent<HealthBar> ().livesRemaining != null) {
			GetComponent<HealthBar> ().livesRemaining.text = text;
		}
	}

    void Update() {
        if (isDead) {
			if (numDeaths < 2) {
				RpcDisplayDeathText ("YOU DIED" + "\n" + "Respawning in " + Mathf.Round (RESPAWN_TIME - (Time.time - deathTime)));
			} else {
				RpcDisplayDeathText ("YOU LOSE");
				RpcDisplayLivesCount ("Lives: 0");
			}

            if (Time.time - deathTime > RESPAWN_TIME && numDeaths < 2) {
                PlayerRespawn();
				GetComponent<ExtraWeaponController> ().CmdEndJetpackParticles (GetComponent<PlayerController>().playerUniqueID);
				numDeaths++;

				RpcDisplayLivesCount ("Lives: " + (3 - numDeaths).ToString ());
				RpcDisplayDeathText ("");
            }
        }
    }
}