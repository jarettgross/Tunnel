using UnityEngine;
using System.Collections;
//using UnityEngine.Networking;

public class GunAnimation : MonoBehaviour
{
    //need to be equal to the cooldown time of gun
    public float cooldown = 0.3f;

    //the remaining time to start the next animation
    float cooldownRemaining = 0;

	private Animation animator;

    // Use this for initialization
    void Start () {
        //network animation(not finished)
        //animator = GetComponent<Animator> ();

        //local animation(current choice)
        animator = GetComponent<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
        //code for network animation
        //if (!isLocalPlayer) {
        //    return;
        //}

        cooldownRemaining -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && cooldownRemaining <= 0)    //if shooting is performed
        {
            cooldownRemaining = cooldown;           //set the remaining time

            animator.Stop();            //stop the current animation
            animator.Play();            //start a new animation
            
            
            //StartCoroutine("Shoot");
        }
    }

	/*public IEnumerator Shoot () {

		animator.SetBool("Firing", true);

		yield return null;

		animator.SetBool("Firing", false);

	}*/
}
