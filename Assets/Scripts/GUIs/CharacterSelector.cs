using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour {

	private int listIndex = 0;
	private int numClasses;

	public Text characterTitle;

	private Transform container; //Holds all character models
	private const float CIRCLE_RADIUS = 7.0f;

	private bool isRotating = false;
	private float rotateTime = 0.0f;
	private float rotateTimeScale = 0.7f;
	private Quaternion initialRotation;
	private Quaternion finalRotation;
	private int rotationDirection;

	public GameObject[] characters;

	private bool isNextButtonClick = false;
	private bool isPrevButtonClick = false;

	private bool isReady = false;

	public Button readyButton;
    public Button startGameButton;

	enum PlayerState {
		NOT_CONNECTED,
		CONNECTED,
		READY
	};

    void Start () {
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
		if (!isRotating) { //then check for inputs
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

		if (Input.GetKeyDown (KeyCode.Space)) {
			SetReady ();
		}

		if (GameObject.Find ("Network Manager").GetComponent<CustomNetworkManager> () != null) {
			GameObject.Find ("Network Manager").GetComponent<CustomNetworkManager> ().client.connection.playerControllers [0].gameObject.GetComponent<SceneController> ().CharacterSelectionPlayerStatuses ();
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
		characterTitle.text = characters [listIndex].GetComponent<CharacterClass>().className;
	}


	public void SetReady() {
		isReady = !isReady;
		if (isReady) {
			readyButton.image.color = Color.green;
            startGameButton.gameObject.SetActive(true);
			GameObject currentClass = characters [listIndex]; //gets character class based on chosen prefab on select screen
			int classIndex = currentClass.GetComponent<CharacterClass>().classIndex;
			//GameObject currentClass = GameObject.Find("Characters").GetComponent<CharacterContainer>().GetClass(listIndex);
			GameObject.Find ("Network Manager").GetComponent<CustomNetworkManager> ().client.connection.playerControllers [0].gameObject.GetComponent<SceneController> ().CharacterSelectionScreenReady (classIndex);
		} else {
			readyButton.image.color = Color.red;
            startGameButton.gameObject.SetActive(false);
            GameObject.Find ("Network Manager").GetComponent<CustomNetworkManager> ().client.connection.playerControllers [0].gameObject.GetComponent<SceneController> ().CharacterSelectionScreenNotReady ();
		}
	}

    public void StartGame()
    {
        if (isReady)
        {
            GameObject.Find("Network Manager").GetComponent<CustomNetworkManager>().client.connection.playerControllers[0].gameObject.GetComponent<SceneController>().CharacterSelectionScreenStartGame();
        }
    }
}
