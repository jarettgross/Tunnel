using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : NetworkBehaviour {

	public CustomNetworkManager networkManager;

	private const int characterSelectionMenueSceneId = 1;
	private const int worldSceneId = 2;

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

	public void CharacterSelectionScreenReady(int classIndex) {
		Debug.Log("Character Ready with class " + classIndex);
		GameObject characters = GameObject.Find("Characters");
		CharacterContainer container = characters.GetComponent<CharacterContainer>();
		GameObject co = container.GetClass(classIndex);
		CharacterClass cc = co.GetComponent<CharacterClass>();
		GetComponent<CharacterClass>().Initialize(cc);
		CmdUpdateClass(classIndex);
		CmdCharacterReady ();
	}

	[Command]
	public void CmdUpdateClass(int classIndex) {
		GetComponent<CharacterClass>().Initialize(GameObject.Find("Characters").GetComponent<CharacterContainer>().GetClass(classIndex).GetComponent<CharacterClass>());
	}

	public void CharacterSelectionScreenNotReady() {
		CmdCharacterNotReady ();
	}

    public void CharacterSelectionScreenStartGame()
    {
        CmdCharacterStartGame();
    }

    [Command]
	private void CmdCharacterReady() {
		networkManager.CharacterSelectionScreenPlayerReady (gameObject);
	}

	[Command]
	private void CmdCharacterNotReady() {
		networkManager.CharacterSelectionScreenPlayerNotReady (gameObject);
	}

    [Command]
    private void CmdCharacterStartGame()
    {
        networkManager.CharacterSelectionScreenStartGame();
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
