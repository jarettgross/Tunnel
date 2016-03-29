using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : NetworkBehaviour {

	public CustomNetworkManager networkManager;

	private string characterSelectionMenueScene = "Character Select Menu";
	private string worldScene = "Deformable Scene";

	public override void OnStartLocalPlayer() {
		DontDestroyOnLoad (gameObject);
		LoadCharacterSelectionScreen ();
	}

	/* * * * * * * * * * * * * * * * * 
	 * Start Character Selection Menu Messages
	 * * * * * * * * * * * * * * * * */

	private void LoadCharacterSelectionScreen() {
		//Application.LoadLevel (characterSelectionMenueScene);
		SceneManager.LoadScene (characterSelectionMenueScene);
	}

	public void CharacterSelectionScreenReady() {
		CmdCharacterReady ();
	}

	[Command]
	private void CmdCharacterReady() {
		//RpcLoadWorld ();
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
		gameObject.GetComponent<PlayerSetup> ().EnableComponents ();
		//Application.LoadLevel (worldScene);
		SceneManager.LoadScene (worldScene);
	}

	[ClientRpc]
	public void RpcRegisterPlayer(GameObject otherPlayer) {
		DontDestroyOnLoad (otherPlayer);
	}

}
