using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager {

	public int requiredPlayers;

	public GameObject terrainManagerPrefab;

	//private Dictionary<NetworkConnection, GameObject> connections;
	private Dictionary<GameObject, int> players;
    private Dictionary<NetworkConnection, GameObject> connections;

	// The scene clients should display first
	private PlayerState[] playerStates;

	private bool terrainSpawned;

	// player spawns
    private const int NUM_SPAWNS = 4;
    private Vector3[] spawns;

	enum PlayerState {
		NOT_CONNECTED,
		CONNECTED,
		READY
	};


	override public void OnStartServer() {
		Debug.Log ("Server Starting");

		players = new Dictionary<GameObject, int>();
        connections = new Dictionary<NetworkConnection, GameObject>();
		playerStates = new PlayerState[requiredPlayers];
		for (int i = 0; i < requiredPlayers; i++) {
			playerStates[i] = PlayerState.NOT_CONNECTED;
		}
		terrainSpawned = false;
		
        // FIXME better spawns
        spawns = new Vector3[NUM_SPAWNS];
        spawns[0] = new Vector3(5, 20, 5);
        spawns[1] = new Vector3(75, 20, 75);
        spawns[2] = new Vector3(5, 20, 75);
        spawns[3] = new Vector3(75, 20, 5);
	}

	override public void OnServerConnect(NetworkConnection conn) {
		//OnServerAddPlayer (conn, id++);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        int id = GetNextId();
        if (id == -1)
        {
            Debug.LogError("Game already full, too many players");
        }

        // set spawn position
        Vector3 startPosition = spawns[id % NUM_SPAWNS];

		GameObject player = (GameObject)Instantiate(playerPrefab, startPosition, Quaternion.identity);
		player.GetComponent<TerrainController> ().networkManager = this;
		player.GetComponent<SceneController> ().networkManager = this;
		player.GetComponent<ExtraWeaponController> ().networkManager = this;

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		// Alert new player of existing players
		RegisterPlayers(player);

		players.Add(player, id);
        connections.Add(conn, player);
        playerStates[id] = PlayerState.CONNECTED;

		Debug.Log ("Player " + id + " joined");
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, UnityEngine.Networking.PlayerController player) {
		base.OnServerRemovePlayer (conn, player);
		int index = 0;
		players.TryGetValue(player.gameObject, out index);

		playerStates[index] = PlayerState.NOT_CONNECTED;
		players.Remove (player.gameObject);
        connections.Remove(conn);
        Debug.Log("Player " + index + " removed");
	}

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        int index = 0;
        GameObject player;
        connections.TryGetValue(conn, out player);
        players.TryGetValue(player, out index);

        playerStates[index] = PlayerState.NOT_CONNECTED;
        players.Remove(player);
        connections.Remove(conn);
        Debug.Log("Player " + index + " disconnected");
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
		int index = -1;
		players.TryGetValue(player, out index);
        Debug.Log("Ready: " + index);
        if (index >= 0)
        {
            playerStates[index] = PlayerState.READY;
        }
        else
        {
            Debug.Log("Error: Missing Player");
        }
	}

	public void CharacterSelectionScreenPlayerNotReady(GameObject player) {
		int index = -1;
		players.TryGetValue(player, out index);
        if (index >= 0)
        {
            playerStates[index] = PlayerState.CONNECTED;
        }
        else
        {
            Debug.Log("Error: Missing Player");
        }
	}

    /*
	 * Alerts server player that someone started the game.
     * All players need to be ready for a game to start
	 */
    public void CharacterSelectionScreenStartGame()
    {
        foreach (PlayerState state in playerStates)
        {
            if (state == PlayerState.CONNECTED)
            {
                return;
            }
        }
        LoadWorld();
    }

        /* * * * * * * * * * * * * * * * * 
		 * World Messages
		 * * * * * * * * * * * * * * * * */

        /*
        * Tells all connected clients to load the world 
        */
    public void LoadWorld() {
		foreach (KeyValuePair<GameObject, int> player in players) {
            if (player.Key != null)
            {
                PlayerState state = playerStates[player.Value];
                if (state == PlayerState.READY || state == PlayerState.CONNECTED)
                {
                    player.Key.GetComponent<SceneController>().RpcLoadWorld();
                }
            }
		}
	}

	/*
	 * Spawns terrain on the clients
	 */ 
	public void SpawnTerrain() {
		if (!terrainSpawned) {
			GameObject terrainManager = (GameObject)Instantiate(terrainManagerPrefab, Vector3.zero, Quaternion.identity);
			NetworkServer.Spawn(terrainManager);
			terrainSpawned = true;
		}
	}

	/*
	 * Sends a deformation to all connected clients
	 */
	public void SendDeformation(Deformation deformation) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<TerrainController> ().RpcDeform (deformation.Position, deformation.GetDeformationType(), deformation.Radius);
		}
	}

	public void SendParticleInfo(Vector3 position, Vector3 hitDirection) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<TerrainController> ().RpcTerrainParticles (position, hitDirection);
		}
	}

	public void SendGrenadeParticleInfo(Vector3 pos, Vector3 dir) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<ExtraWeaponController> ().RpcGrenadeParticles (pos, dir);
		}
	}

	public void SendPickupInfo() {
		Vector3 pickupPos = new Vector3 (Random.Range (5, 75), Random.Range (2, 20), Random.Range (5, 75));
		foreach (GameObject player in players.Keys) {
			player.GetComponent<TerrainController> ().RpcSpawnPickupBox(pickupPos);
		}
	}

	public void SendJetpackParticleID(NetworkInstanceId id) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<ExtraWeaponController> ().RpcJetpackParticles (id);
		}
	}

	public void SendEndJetpackParticleID(NetworkInstanceId id) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<ExtraWeaponController> ().RpcEndJetpackParticles (id);
		}
	}

	public void SendInvisibilityID(NetworkInstanceId id, bool isInvisible) {
		foreach (GameObject player in players.Keys) {
			player.GetComponent<ExtraWeaponController> ().RpcInvisibility (id, isInvisible);
		}
	}


		/* * * * * * * * * * * * * * * * * 
		 * Player Messages
		 * * * * * * * * * * * * * * * * */

	/*
	 * Called when a player has died
	 */
	//public void PlayerDied(GameObject player) {
	//	player.transform.position = new Vector3(5, 20, 5);
	//}
}
