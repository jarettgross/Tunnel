using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : NetworkBehaviour {

	public CustomNetworkManager networkManager;

	private const int characterSelectionMenueSceneId = 1;
	private const int worldSceneId = 2;

	private CharacterClass characterClass;

	public override void OnStartLocalPlayer() {
		DontDestroyOnLoad (gameObject);
		LoadCharacterSelectionScreen ();
	}

	void OnLevelWasLoaded(int level) {
		switch (level) {
		case characterSelectionMenueSceneId:
			break;

		case worldSceneId:
			CmdWorldLoaded();
			break;
		}
	}

	/* * * * * * * * * * * * * * * * * 
	 * Start Character Selection Menu Messages
	 * * * * * * * * * * * * * * * * */

	private void LoadCharacterSelectionScreen() {
		SceneManager.LoadScene (characterSelectionMenueSceneId);
	}

	public void CharacterSelectionScreenReady(GameObject _characterClass) {
		characterClass = _characterClass.GetComponent<CharacterClass>();
		CmdCharacterReady ();
	}

	[Command]
	private void CmdCharacterReady() {
		networkManager.CharacterSelectionScreenPlayerReady (gameObject);
	}


	/* * * * * * * * * * * * * * * * * 
	 * Start World Messages
	 * * * * * * * * * * * * * * * * */

	[ClientRpc]
	public void RpcLoadWorld() {
		if (!isLocalPlayer)
			return;
		
		LoadWorld ();
	}

	private void LoadWorld() {
		SceneManager.LoadScene (worldSceneId);
	}
		
	/*
	 * Initialize player components for game
	 */ 
	public void ReadyPlayer() {
		if (!isLocalPlayer)
			return;

		CharacterClass cc = gameObject.AddComponent<CharacterClass>();
		cc.Initialize(characterClass);

		gameObject.GetComponent<PlayerSetup>().EnableComponents ();
		gameObject.GetComponent<TerrainController>().Initialize();
		gameObject.GetComponent<WeaponController>().Initialize();
		gameObject.GetComponent<HealthBar>().Initialize();
		gameObject.GetComponent<PlayerController>().Initialize();
	}

	[ClientRpc]
	public void RpcRegisterPlayer(GameObject otherPlayer) {
		DontDestroyOnLoad (otherPlayer);
	}

	[Command]
	public void CmdWorldLoaded() {
		networkManager.SpawnTerrain();
	}

}
