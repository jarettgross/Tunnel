using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager {

	public int requiredPlayers;

	public GameObject terrainManagerPrefab;

	//private Dictionary<NetworkConnection, GameObject> connections;
	private Dictionary<GameObject, int> players;

	// The scene clients should display first
	private PlayerState[] playerStates;

	enum PlayerState {
		NOT_CONNECTED,
		CONNECTED,
		READY
	};


	override public void OnStartServer() {
		Debug.Log ("Server Starting");

		players = new Dictionary<GameObject, int>();
		playerStates = new PlayerState[requiredPlayers];
		for (int i = 0; i < requiredPlayers; i++) {
			playerStates[i] = PlayerState.NOT_CONNECTED;
		}
	}

	override public void OnServerConnect(NetworkConnection conn) {
		//OnServerAddPlayer (conn, id++);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
		GameObject player = (GameObject)Instantiate(playerPrefab, new Vector3 (5, 20, 5), Quaternion.identity);
		player.GetComponent<TerrainController> ().networkManager = this;
		player.GetComponent<SceneController> ().networkManager = this;

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		// Alert new player of existing players
		RegisterPlayers(player);

		int id = GetNextId();
		if (id == -1) {
			Debug.LogError("Game already full, too many players");
		}
		

		players.Add(player, id);

		Debug.Log ("Player " + players.Count + " joined");
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, UnityEngine.Networking.PlayerController player) {
		base.OnServerRemovePlayer (conn, player);
		int index = 0;
		players.TryGetValue(player.gameObject, out index);

		playerStates[index] = PlayerState.NOT_CONNECTED;
		players.Remove (player.gameObject);
	}

	private int GetNextId() {
		for (int i = 0; i < requiredPlayers; i++) {
			if (playerStates[i] == PlayerState.NOT_CONNECTED)
				return i;
		}

		return -1;
	}

	/* * * * * * * * * * * * * * * * * 
	 * All network messages go below here
	 * * * * * * * * * * * * * * * * */


		/* * * * * * * * * * * * * * * * * 
		 * Connection Messages
		 * * * * * * * * * * * * * * * * */

	/*
	 * Registers existing players with a new player who has joined
	 */ 
	private void RegisterPlayers(GameObject _player) {
		foreach (GameObject player in players.Keys) {
			_player.GetComponent<SceneController> ().RpcRegisterPlayer(player);
			player.GetComponent<SceneController> ().RpcRegisterPlayer(_player);
		}
	}


		/* * * * * * * * * * * * * * * * * 
		 * Character Selection Screen Messages
		 * * * * * * * * * * * * * * * * */

	/*
	 * Alerts server player in character selection screen is ready
	 */
	public void CharacterSelectionScreenPlayerReady(GameObject player) {
		int index = 0;
		players.TryGetValue(player, out index);
		playerStates[index] = PlayerState.READY;

		foreach (PlayerState state in playerStates) {
			if (state != PlayerState.READY)
				return;
		}

		LoadWorld();
	}

	public void CharacterSelectionScreenPlayerNotReady(GameObject player) {
		int index = 0;
		players.TryGetValue(player, out index);
		playerStates[index] = PlayerState.CONNECTED;
	}


		/* * * * * * * * * * * * * * * * * 
		 * World Messages
		 * * * * * * * * * * * * * * * * */

	/*
	* Tells all connected clients to load the world 
	*/
	public void LoadWorld() {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<SceneController> ().RpcLoadWorld();
		}
	}

	/*
	 * Spawns terrain on the clients
	 */ 
	public void SpawnTerrain() {
		GameObject terrainManager = (GameObject)Instantiate(terrainManagerPrefab, Vector3.zero, Quaternion.identity);
		NetworkServer.Spawn(terrainManager);
	}

	/*
	 * Sends a deformation to all connected clients
	 */
	public void SendDeformation(Deformation deformation) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<TerrainController> ().RpcDeform (deformation.Position, deformation.GetDeformationType(), deformation.Radius);
		}
	}

		/* * * * * * * * * * * * * * * * * 
		 * Player Messages
		 * * * * * * * * * * * * * * * * */

	/*
	 * Called when a player has died
	 */
	public void PlayerDied(GameObject player) {
		player.transform.position = new Vector3(5, 20, 5);
	}
}
