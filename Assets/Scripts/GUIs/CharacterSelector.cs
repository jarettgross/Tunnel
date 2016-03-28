using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterSelector : NetworkBehaviour {

	private int listIndex = 0;
	private int numClasses;

	public Text characterTitle;

	private Transform container; //Holds all character models
	private const float CIRCLE_RADIUS = 8.0f;

	private bool isRotating = false;
	private float rotateTime = 0.0f;
	private float rotateTimeScale = 0.7f;
	private Quaternion initialRotation;
	private Quaternion finalRotation;
	private int rotationDirection;

	private bool isNextButtonClick = false;
	private bool isPrevButtonClick = false;

	void Start () {
		GameObject[] characters = Resources.LoadAll<GameObject> ("CharacterPrefabs");
		numClasses = characters.Length;
		container = GameObject.Find ("CharacterContainer").transform;

		for (int i = 0; i < numClasses; i++) {
			float circleLoc = i / (float)numClasses;
			float angle = circleLoc * Mathf.PI * 2;
			float x = Mathf.Sin (angle) * CIRCLE_RADIUS;
			float z = Mathf.Cos (angle) * CIRCLE_RADIUS;
			Vector3 pos = new Vector3 (x, container.position.y, -z);
			GameObject c = Instantiate (characters [i], pos, Quaternion.identity) as GameObject;
			c.transform.SetParent (container);
		}
	}

	void Update() {
		if (!isRotating) {
			if (Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.D) || isNextButtonClick) {
				isNextButtonClick = false;
				rotationDirection = 1;
				initialRotation = container.rotation;
				finalRotation = container.rotation * Quaternion.AngleAxis (rotationDirection * 360.0f / numClasses, new Vector3 (0, 1, 0));
				isRotating = true;
				SetCharacterText ();
			}
			
			if (Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.A) || isPrevButtonClick) {
				isPrevButtonClick = false;
				rotationDirection = -1;
				initialRotation = container.rotation;
				finalRotation = container.rotation * Quaternion.AngleAxis (rotationDirection * 360.0f / numClasses, new Vector3 (0, 1, 0));
				isRotating = true;
				SetCharacterText ();
			}
		}

		if (isRotating) {
			ChangeCharacter ();
		}
	}

	//Switch character
	private void ChangeCharacter() {
		container.rotation = Quaternion.Slerp (initialRotation, finalRotation, rotateTime / rotateTimeScale);
		rotateTime += Time.deltaTime;
		if (Quaternion.Angle(container.rotation, finalRotation) < 0.1f) {
			isRotating = false;
			rotateTime = 0.0f;
		}
	}

	public void PrevButtonChangeCharacter() {
		isPrevButtonClick = true;
	}

	public void NextButtonChangeCharacter() {
		isNextButtonClick = true;
	}

	private void SetCharacterText() {
		listIndex += rotationDirection;
		if (listIndex < 0) {
			listIndex = numClasses - 1;
		} else if (listIndex > numClasses - 1) {
			listIndex = 0;
		}

		switch (listIndex) {
		case 0:
			characterTitle.text = "All-Around";
			break;
		case 1:
			characterTitle.text = "Mobility";
			break;
		case 2:
			characterTitle.text = "Long-Range";
			break;
		case 3:
			characterTitle.text = "Stealth";
			break;
		case 4:
			characterTitle.text = "Traps";
			break;
		}
	}


	public void PlayGame() {
		GameObject.Find ("Network Manager").GetComponent<CustomNetworkManager> ().LoadWorld ();
		//gameObject.GetComponent<SceneController> ().CharacterSelectionScreenReady();
	}
}
