using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GunAnimation : MonoBehaviour
{
    public float cooldown = 0.3f;

    float cooldownRemaining = 0;

	private Animator animator;

    // Use this for initialization
    void Start () {
		animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
        //if (!isLocalPlayer) {
        //    return;
        //}

        cooldownRemaining -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && cooldownRemaining <= 0)
        {
            cooldownRemaining = cooldown;
			Debug.Log ("Shooting");
			StartCoroutine("Shoot");
        }
    }

	public IEnumerator Shoot () {

		animator.SetBool("Firing", true);

		yield return null;

		animator.SetBool("Firing", false);

	}
}
