using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using CoherentNoise;
using CoherentNoise.Generation;
using CoherentNoise.Generation.Fractal;

public class TerrainManager : NetworkBehaviour {

	public int seed;
	public float scale;
	public float noiseScale;

	[Range(-2, 2)]
	public float isolevel;
	public int sizeX, sizeY, sizeZ;

	public int octaves;
	[Range(0, 1)]
	public float persistance;
	public float lacunarity;

	public Vector2 offset;

	public Transform grapplingHook;
	public GameObject worldPrefab;
	/*
	 * Privates
	 */

	private World world;

	private Function functionNoise;
	private PerlinGenerator perlinNoise;

	private Generator noise;

	private NetworkConnection conn;

	// Use this for initialization
	void Start() {

		if (!isClient)
			return;

		DontDestroyOnLoad(this);

		UnityEngine.Debug.Log ("Starting Terrain Creation");

		functionNoise = new Function (CutoffFunc);

		//perlinNoise = new PerlinGenerator (81, 11, 81, noiseScale, seed, offset, octaves, persistance, lacunarity);

		Stopwatch timer = new Stopwatch ();
		timer.Start ();
		perlinNoise = new PerlinGenerator (sizeX * Chunk.CHUNK_X + 1, sizeY * Chunk.CHUNK_Y + 1, sizeZ * Chunk.CHUNK_Z + 1, noiseScale, seed, offset, octaves, persistance, lacunarity);
		timer.Stop ();

		System.TimeSpan ts = timer.Elapsed;

		UnityEngine.Debug.Log("RunTime " + ts.TotalMilliseconds);

		noise = functionNoise + perlinNoise;


		GameObject worldObject = Instantiate(worldPrefab);
		worldObject.name = "World";
		world = worldObject.GetComponent<World> ();
		world.Initialize (sizeX, sizeY, sizeZ, noise, isolevel);
		world.StartWorld ();


		
		GameObject.Find("Network Manager").GetComponent<CustomNetworkManager>().client.connection.playerControllers[0].gameObject.GetComponent<TerrainController>().DisplayWorld(gameObject);
	}

	private float CutoffFunc(float x, float y, float z) {
		if (y <= 16)
			return 0;
		return 1;
	}

	public void Deform(Deformation deformation) {
		world.Deform (deformation);
	}

	public void UpdateWorld(Vector3 position) {
		world.UpdateWorld (position);
	}

	public void Redraw() {
		perlinNoise = new PerlinGenerator (sizeX * Chunk.CHUNK_X + 1, sizeY * Chunk.CHUNK_Y + 1, sizeZ * Chunk.CHUNK_Z + 1, noiseScale, seed, offset, octaves, persistance, lacunarity);
		noise = functionNoise + perlinNoise;
		world.DestroyWorld ();
		world.UpdateNoise (noise);
		world.AddTerrain ();
	}
}
