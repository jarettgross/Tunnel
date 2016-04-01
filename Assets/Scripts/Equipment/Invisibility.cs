using UnityEngine;
using System.Collections;

public class Invisibility : MonoBehaviour {

	public GameObject owner;

	private float invisibilityRemaining; //invisiblity remaining in seconds
	private bool isInvisible;
	private bool isChanging; //currently going visible/invisible

	private float currentAlpha; //current transparency of material

	private float lerpTime; 
	private float lerpScale; //how long it takes to turn invisible

	// Use this for initialization
	void Start () {
		invisibilityRemaining = 60.0f;
		isInvisible = false;
		lerpTime = 0.0f;
		lerpScale = 0.7f;
	}
	
	// Update is called once per frame
	void Update () {
		if (isChanging) {
			if (isInvisible) {
				MakeInvisible ();
			} else {
				MakeVisible ();
			}
		}
	}

	public void ToggleInvisiblity() {
		isInvisible = !isInvisible;
		currentAlpha = owner.GetComponent<Renderer> ().material.color.a;
		isChanging = true;
	}

	private void MakeInvisible() {
		Color tempColor = owner.GetComponent<Renderer>().material.color;
		tempColor.a = Mathf.Lerp (currentAlpha, 0.0f, lerpTime / lerpScale); //change alpha
		owner.GetComponent<Renderer> ().material.color = tempColor;
		lerpTime += Time.deltaTime;
		if (owner.GetComponent<Renderer> ().material.color.a == 0.0f) { //once alpha is 0, done turning invisible
			isChanging = false;
			lerpTime = 0.0f;
		}
		invisibilityRemaining -= Time.deltaTime;
	}

	private void MakeVisible() {
		Color tempColor = owner.GetComponent<Renderer>().material.color;
		tempColor.a = Mathf.Lerp (currentAlpha, 1.0f, lerpTime / lerpScale); //change alpha
		owner.GetComponent<Renderer> ().material.color = tempColor;
		lerpTime += Time.deltaTime;
		if (owner.GetComponent<Renderer> ().material.color.a == 1.0f) { //once alpha is 1, done turning visible
			isChanging = false;
			lerpTime = 0.0f;
		}
	}
}
