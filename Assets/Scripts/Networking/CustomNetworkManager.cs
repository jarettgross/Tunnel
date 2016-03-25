using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager {

	public GameObject terrainManager;

	//private Dictionary<NetworkConnection, GameObject> connections;
	private List<GameObject> players;

	private static short id = 0;

	override public void OnStartServer() {
		Debug.Log ("Server Starting");

		//connections = new Dictionary<NetworkConnection, GameObject> ();
		players = new List<GameObject> ();

		ServerChangeScene ("Character Select Menu");
	}

	override public void OnServerConnect(NetworkConnection conn) {
		//OnServerAddPlayer (conn, id++);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
//		GameObject terrain = (GameObject)Instantiate (terrainManager, Vector3.zero, Quaternion.identity);
//		terrain.name = "terrainManager";

		string info = "";
		info += conn.address + "\n";
		info += conn.connectionId + "\n";
		info += playerControllerId + "\n";
		Debug.Log (info);


		GameObject player = (GameObject)Instantiate(playerPrefab, new Vector3 (5, 20, 5), Quaternion.identity);
		player.GetComponent<TerrainController> ().networkManager = this;
		player.GetComponent<Renderer> ().enabled = false;
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		//connections.Add (conn, player);
		players.Add(player);
		DontDestroyOnLoad (player);

		Debug.Log ("Player " + players.Count + " joined");
	}

	public void SendDeformation(Deformation deformation) {
		Debug.Log ("Sending deformation from server");

		//foreach (NetworkConnection conn in connections.Keys) {
		foreach (GameObject player in players) {
			//GameObject player = connections [conn];
			player.GetComponent<TerrainController> ().RpcDeform (deformation.Position, deformation.GetDeformationType(), deformation.Radius);
		}
	}

	public void LoadWorld() {
		ServerChangeScene ("Deformable Scene");

		foreach (GameObject player in players) {
			//GameObject player = connections [conn];
			player.GetComponent<TerrainController> ().RpcDisplayWorld();
		}
	}
}
