﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneController : NetworkBehaviour {

	public CustomNetworkManager networkManager;

	private const int characterSelectionMenueSceneId = 2;
	private const int worldSceneId = 3;

	public int worldSeed;

	public override void OnStartLocalPlayer() {
		DontDestroyOnLoad (gameObject);
		LoadCharacterSelectionScreen ();
	}

	void OnLevelWasLoaded(int level) {
		switch (level) {
		case characterSelectionMenueSceneId:
			break;

		case worldSceneId:
			CmdServerSeed ();
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

	public void CharacterSelectionPlayerStatuses() {
		CmdStatuses ();
	}

	[Command]
	public void CmdStatuses() {
		networkManager.SendReadyPlayerInfo ();
	}

	[ClientRpc]
	public void RpcStatuses(string statuses) {
		if (GameObject.Find ("CharacterSelection") != null) {
			Text[] texts = GameObject.Find ("CharacterSelection").GetComponentsInChildren<Text> ();
			foreach (Text t in texts) {
				if (t.name == "OthersReady") {
					t.text = statuses;
				}
			}
		}
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
		gameObject.GetComponent<PlayerGUI> ().Initialize ();
	}

	[ClientRpc]
	public void RpcRegisterPlayer(GameObject otherPlayer) {
		DontDestroyOnLoad (otherPlayer);
	}

	[Command]
	public void CmdWorldLoaded() {
		networkManager.SpawnTerrain();
	}

	[Command]
	public void CmdServerSeed() {
		networkManager.SendServerSeed ();
	}

	[ClientRpc]
	public void RpcSeed(int seed) {
		worldSeed = seed;
		//Can optionally set other world variables here (lacunarity, persistance, etc.)
	}

}
