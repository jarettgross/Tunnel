using UnityEngine;
using UnityEngine.Networking;

public class HasHealth : NetworkBehaviour {

    private float hitPoints;

    [SyncVar]
    private float currentHealth;

	void Awake() {
		SetDefaults();
	}

	void SetDefaults() {
		hitPoints = 100;
		currentHealth = hitPoints;
	}

    public void ReceiveDamage(float amt)
    {   
        currentHealth -= amt;
        if(currentHealth <= 0)
        {
            Die();
        }

		Debug.Log("Took damage. Health now: " + currentHealth);
    }

	public float HealthRatio() {
		return currentHealth / hitPoints;
	}

    void Die()
    {
        /*foreach(Transform child in transform)
        {
            Debug.Log("Destroyed Child: " + child.name);
            Destroy(child.gameObject);
        }*/
       
        Destroy(gameObject);
    }
}
