using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CharacterContainer : NetworkBehaviour {

	public GameObject[] classes;

	// Use this for initialization
	void Awake () {
		DontDestroyOnLoad(gameObject);

		if (isServer) {
			SpawnClasses();
		}
	}

	[Server]
	void SpawnClasses() {
		for (int i = 0; i < classes.Length; i++) {
			DontDestroyOnLoad(classes[i]);
			classes[i].SetActive(false);
			NetworkServer.Spawn(classes[i]);
		}
	}

	public GameObject GetClass(int index) {
		return classes[index];
	}
}
