using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using CoherentNoise;
using CoherentNoise.Generation;
using CoherentNoise.Generation.Fractal;

public class CubesRenderer : MonoBehaviour {

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

	// Use this for initialization
	void Start () {

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
	}

	public void Update() {

		/*
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Transform transform = Camera.main.transform;
			Ray ray = new Ray (transform.position, transform.forward); 

			if (Physics.Raycast(ray, out hit)) {
				Vector3 position = hit.point;
				Deform (position);
			}
		}
		*/
	}


	private float CutoffFunc(float x, float y, float z) {
		if (y <= 16)
			return 0;
		return 1;
	}

	private void Deform(Vector3 position) {

		Deformation deformation = new Deformation(position, Deformation.DeformationType.Cube, 1);

		world.Deform (deformation);
	}

	public void Redraw() {
		perlinNoise = new PerlinGenerator (sizeX * Chunk.CHUNK_X + 1, sizeY * Chunk.CHUNK_Y + 1, sizeZ * Chunk.CHUNK_Z + 1, noiseScale, seed, offset, octaves, persistance, lacunarity);
		noise = functionNoise + perlinNoise;
		world.DestroyWorld ();
		world.UpdateNoise (noise);
		world.AddTerrain ();
	}
}
