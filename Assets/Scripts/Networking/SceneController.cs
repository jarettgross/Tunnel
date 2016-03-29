using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : NetworkBehaviour {

	public CustomNetworkManager networkManager;

	public override void OnStartLocalPlayer() {
		DontDestroyOnLoad (gameObject);

		LoadWorld ();
		//StartCoroutine (LoadLevelWait ());
	}

//	IEnumerator LoadLevelWait() {
//		//yield return new WaitForSeconds (5);
//		//Application.LoadLevel ("Character Select Menu");
//		LoadWorld();
//	}

//	public void RpcLoadCharacterSelectionScreen() {
//		Application.LoadLevel ("Character Select Menu");
//	}

	public void CharacterSelectionScreenReady() {
		CmdCharacterReady ();
	}

//	private void PrepareCharacter() {
//		gameObject.AddComponent<HasHealth> ();
//		gameObject.AddComponent<HealthBar> ();
//		gameObject.AddComponent<PerformAttack> ();
//		gameObject.AddComponent<SimpleSmoothMouseLook> ();
//		gameObject.AddComponent<PlayerController> ();
//		gameObject.AddComponent<PlayerSetup> ();
//		gameObject.AddComponent<TerrainController> ();
//	}

	[Command]
	private void CmdCharacterReady() {
		RpcLoadWorld ();
	}

	[ClientRpc]
	public void RpcLoadWorld() {
		if (!isLocalPlayer)
			return;
		
		LoadWorld ();
	}

	private void LoadWorld() {
		Application.LoadLevel ("Deformable Scene");
		//gameObject.GetComponent<TerrainController> ().DisplayWorld ();
	}

	[ClientRpc]
	public void RpcRegisterPlayer(GameObject otherPlayer) {
		DontDestroyOnLoad (otherPlayer);
	}

}
