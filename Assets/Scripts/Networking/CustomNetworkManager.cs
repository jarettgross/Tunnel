using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager {

	public int requiredPlayers;

	//private Dictionary<NetworkConnection, GameObject> connections;
	private List<GameObject> players;

	// The scene clients should display first
	private string startScene = "Character Select Menu";
	private int playersReady;

	override public void OnStartServer() {
		Debug.Log ("Server Starting");

		players = new List<GameObject> ();
		playersReady = 0;
	}

	override public void OnServerConnect(NetworkConnection conn) {
		//OnServerAddPlayer (conn, id++);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
		GameObject player = (GameObject)Instantiate(playerPrefab, new Vector3 (5, 20, 5), Quaternion.identity);
		player.GetComponent<TerrainController> ().networkManager = this;
		player.GetComponent<SceneController> ().networkManager = this;

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		// Alert new players of existing players
		RegisterPlayers(player);

		players.Add(player);

		Debug.Log ("Player " + players.Count + " joined");
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, UnityEngine.Networking.PlayerController player) {
		base.OnServerRemovePlayer (conn, player);
		players.Remove (player.gameObject);
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
		foreach (GameObject player in players) {
			_player.GetComponent<SceneController> ().RpcRegisterPlayer(player);
		}
	}


		/* * * * * * * * * * * * * * * * * 
		 * Character Selection Screen Messages
		 * * * * * * * * * * * * * * * * */

	/*
	 * Alerts server player in character selection screen is ready
	 */
	public void CharacterSelectionScreenPlayerReady(GameObject gameObject) {

		if (players.Count != requiredPlayers) {
			return;
		}

		playersReady++;

		if (playersReady == requiredPlayers) {
			LoadWorld ();
		}
	}


		/* * * * * * * * * * * * * * * * * 
		 * World Messages
		 * * * * * * * * * * * * * * * * */

	/*
	* Tells all connected clients to load the world 
	*/
	public void LoadWorld() {
		foreach (GameObject player in players) {
			player.GetComponent<SceneController> ().RpcLoadWorld();
		}
	}

	/*
	 * Sends a deformation to all connected clients
	 */
	public void SendDeformation(Deformation deformation) {
		Debug.Log ("Sending deformation from server");

		foreach (GameObject player in players) {
			player.GetComponent<TerrainController> ().RpcDeform (deformation.Position, deformation.GetDeformationType(), deformation.Radius);
		}
	}
}
