using UnityEngine;
using UnityEngine.Networking;

public class HasHealth : NetworkBehaviour {
    [SerializeField]
    public float hitPoints = 100f;

    [SyncVar]
    public float currentHealth;

    void Awake()
    {
        SetDefaults();
    }

    void SetDefaults()
    {
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
