using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Threading;
using System.Collections.Generic;

using CoherentNoise;
using CoherentNoise.Generation;

public class World : NetworkBehaviour {

	public GameObject wallPrefab;
	public GameObject chunkPrefab;

	public const int CHUNK_FILL_RANGE_HORIZ = 20;
	public const int CHUNK_FILL_RANGE_VERT = 5;


	private int sizeX;
	private int sizeY;
	private int sizeZ;

	private int farX;
	private int farY;
	private int farZ;

	private const int scale = 1;
	private float isolevel;
	private float[] isolevels = new float[] {0.3f, 0.5f};
	private bool[] destructable = new bool[] {false, true};

	private Generator generator;
	private HashSet<Vector3> deformSet;
	private Chunk[ , , ] chunks;


	/*
	 * Threading Stuff
	 */
	volatile bool running;
	ProduceConsume<Chunk> chunkBuildQueue;
	ProduceConsume<Chunk> chunkFinishedQueue;
	ProduceConsume<Deformation> deformationQueue;
	ProduceConsume<Chunk> deformationFinishQueue;

	Thread chunkBuilder;
	Thread deformBuilder;


	public void UpdateNoise(Generator gen) {
		generator = gen;
	}

	public void Initialize(int _sizeX, int _sizeY, int _sizeZ, Generator _generator, float _isolevel) {
		sizeX = _sizeX;
		sizeY = _sizeY;
		sizeZ = _sizeZ;
		farX = sizeX * Chunk.CHUNK_X;
		farY = sizeY * Chunk.CHUNK_Y;
		farZ = sizeZ * Chunk.CHUNK_Z;
		generator = _generator;
		deformSet = new HashSet<Vector3> ();
		chunks = new Chunk[sizeX, sizeY, sizeZ];
		isolevel = _isolevel;

		running = true;
		chunkBuildQueue = new ProduceConsume<Chunk> ();
		chunkFinishedQueue = new ProduceConsume<Chunk> ();
		deformationQueue = new ProduceConsume<Deformation> ();
		deformationFinishQueue = new ProduceConsume<Chunk> ();

		BuildWalls ();
	}

	public void StartWorld() {
		ThreadStart chunk = new ThreadStart (ChunkBuilder);
		chunkBuilder = new Thread (chunk);
		chunkBuilder.Start ();

		ThreadStart deform = new ThreadStart (DeformBuilder);
		deformBuilder = new Thread (deform);
		deformBuilder.Start ();
	}

	public void UpdateWorld(Vector3 position) {
		Chunk chunk;
		while ((chunk = chunkFinishedQueue.ConsumeNonBlock ()) != null) {
			chunk.ApplyMesh ();
		}
			
		while ((chunk = deformationFinishQueue.ConsumeNonBlock ()) != null) {
			chunk.ApplyMesh ();
		}

		CheckChunks (position);
	}

	public void Cleanup() {
		running = false;
		chunkBuilder.Join ();
		deformBuilder.Join ();
	}

	private void CheckChunks(Vector3 position) {
		int minChunkX = (int)(position.x - CHUNK_FILL_RANGE_HORIZ) / Chunk.CHUNK_X;
		int maxChunkX = (int)(position.x + CHUNK_FILL_RANGE_HORIZ) / Chunk.CHUNK_X;
		int minChunkY = (int)(position.y - CHUNK_FILL_RANGE_VERT) / Chunk.CHUNK_Y;
		int maxChunkY = (int)(position.y + CHUNK_FILL_RANGE_VERT) / Chunk.CHUNK_Y;
		int minChunkZ = (int)(position.z - CHUNK_FILL_RANGE_HORIZ) / Chunk.CHUNK_Z;
		int maxChunkZ = (int)(position.z + CHUNK_FILL_RANGE_HORIZ) / Chunk.CHUNK_Z;

		if (minChunkX < 0)
			minChunkX = 0;

		if (minChunkY < 0)
			minChunkY = 0;

		if (minChunkZ < 0)
			minChunkZ = 0;

		if (maxChunkX >= sizeX)
			maxChunkX = sizeX - 1;

		if (maxChunkY >= sizeY)
			maxChunkY = sizeY - 1;

		if (maxChunkZ >= sizeZ)
			maxChunkZ = sizeZ - 1;


		for (int x = minChunkX; x <= maxChunkX; x++) {
			for (int y = 0; y <= maxChunkY; y++) {
				for (int z = minChunkZ; z <= maxChunkZ; z++) {
					if (chunks [x, y, z] == null)
						CreateChunk (x, y, z);
				}
			}
		}
	}

	private void BuildWalls() {
		GameObject floor = Instantiate (wallPrefab);
		floor.transform.localScale = new Vector3 (sizeX, 1, sizeZ);
		floor.transform.position = new Vector3 (sizeX * Chunk.CHUNK_X / 2, 0, sizeZ * Chunk.CHUNK_Z / 2);
		DontDestroyOnLoad(floor);

		GameObject wallMinX = Instantiate (wallPrefab);
		wallMinX.transform.localScale = new Vector3 (sizeX, 1, sizeY);
		wallMinX.transform.rotation = Quaternion.Euler (new Vector3 (90, 0, 0));
		wallMinX.transform.position = new Vector3 (sizeX * Chunk.CHUNK_X / 2, sizeY * Chunk.CHUNK_Y / 2, 0);
		DontDestroyOnLoad(wallMinX);

		GameObject wallMaxX = Instantiate (wallPrefab);
		wallMaxX.transform.localScale = new Vector3 (sizeX, 1, sizeY);
		wallMaxX.transform.rotation = Quaternion.Euler (new Vector3 (-90, 0, 0));
		wallMaxX.transform.position = new Vector3 (sizeX * Chunk.CHUNK_X / 2, sizeY * Chunk.CHUNK_Y / 2, sizeZ * Chunk.CHUNK_Z);
		DontDestroyOnLoad(wallMaxX);

		GameObject wallMinZ = Instantiate (wallPrefab);
		wallMinZ.transform.localScale = new Vector3 (sizeY, 1, sizeZ);
		wallMinZ.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, -90));
		wallMinZ.transform.position = new Vector3 (0, sizeY * Chunk.CHUNK_Y / 2, sizeZ * Chunk.CHUNK_Z / 2);
		DontDestroyOnLoad(wallMinZ);

		GameObject wallMaxZ = Instantiate (wallPrefab);
		wallMaxZ.transform.localScale = new Vector3 (sizeY, 1, sizeZ);
		wallMaxZ.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 90));
		wallMaxZ.transform.position = new Vector3 (sizeX * Chunk.CHUNK_X, sizeY * Chunk.CHUNK_Y / 2, sizeZ * Chunk.CHUNK_Z / 2);
		DontDestroyOnLoad(wallMaxZ);
	}

	public void AddTerrain() {
		for (int x = 0; x < sizeX; x++) {
			for (int y = 0; y < sizeY; y++) {
				for (int z = 0; z < sizeZ; z++) {
					CreateChunk (x, y, z);
				}
			}
		}
	}

	private void CreateChunk(int x, int y, int z) {
		GameObject chunkObject = Instantiate (chunkPrefab);
		chunkObject.name = "chunk";

		Chunk chunk = new Chunk (new Vector3 (x * Chunk.CHUNK_X, y * Chunk.CHUNK_Y, z * Chunk.CHUNK_Z), scale, chunkObject, isolevels.Length);
		chunks[x, y, z] = chunk;

		Debug.Log ("Thread " + Thread.CurrentThread.ManagedThreadId + ": enqueueing chunk at: " + x + ", " + y + ", " + z);
		chunkBuildQueue.Produce (chunk);
	}

	public void Deform(Deformation deformation) {
		deformationQueue.Produce (deformation);
	}

	private void ApplyDeformation(Deformation deformation) {
		foreach (Vector3 vertex in deformation.Apply())
			deformSet.Add (vertex);

		Vector3 position = deformation.Position;
		int radius = deformation.Radius;
		int minX = (int) (position.x - radius) - 1;
		int maxX = (int) (position.x + radius) + 1;

		int minY = (int) (position.y - radius) - 1;
		int maxY = (int) (position.y + radius) + 1;

		int minZ = (int) (position.z - radius) - 1;
		int maxZ = (int) (position.z + radius) + 1;

		for (int x = minX / Chunk.CHUNK_X; x <= maxX / Chunk.CHUNK_X; x++) {
			for (int y = minY / Chunk.CHUNK_Y; y <= maxY / Chunk.CHUNK_Y; y++) {
				for (int z = minZ / Chunk.CHUNK_Z; z <= maxZ / Chunk.CHUNK_Z; z++) {
					DeformChunk (x, y, z, minX, maxX, minY, maxY, minZ, maxZ);
				}
			}
		}
	}

	private void DeformChunk(int x, int y, int z, int minX, int maxX, int minY, int maxY, int minZ, int maxZ) {
		
		if (x < 0 || x >= chunks.GetLength (0))
			return;

		if (y < 0 || y >= chunks.GetLength (1))
			return;

		if (z < 0 || z >= chunks.GetLength (2))
			return;

		if (chunks [x, y, z] == null)
			return;

		chunks[x, y, z].Deform(this, minX, maxX, minY, maxY, minZ, maxZ);
		deformationFinishQueue.Produce (chunks [x, y, z]);
	}

	public void DestroyWorld() {
		for (int x = 0; x < sizeX; x++) {
			for (int y = 0; y < sizeY; y++) {
				for (int z = 0; z < sizeZ; z++) {
					if (chunks [x, y, z] != null) {
						DestroyChunk (x, y, z);
					}
				}
			}
		}
	}

	private void DestroyChunk(int x, int y, int z) {
		Destroy (chunks [x, y, z].ChunkObject);
		chunks [x, y, z] = null;
	}

	private void ChunkBuilder() {
		while (running) {
			try {
				Chunk chunk = chunkBuildQueue.Consume ();
				chunk.Build (this);
				chunkFinishedQueue.Produce (chunk);
			} catch (Exception e) {
				Debug.Log (e.GetBaseException ());
			}
		}
	}

	private void DeformBuilder() {
		while (running) {
			try {
				Deformation deformation = deformationQueue.Consume ();
				ApplyDeformation (deformation);
			} catch (Exception e) {
				Debug.Log (e.GetBaseException());
			}
		}
	}


	/*
	 * Getters
	 */

	public float GetValue(Vector3 position) {
		return GetValue ((int) position.x, (int) position.y, (int) position.z);
	}

	public float GetValue(int x, int y, int z) {
		return generator.GetValue (x, y, z);
	}

	public bool IsBorder(Vector3 position) {
		return IsBorder ((int) position.x, (int) position.y, (int) position.z);
	}

	public bool IsBorder(int x, int y, int z) {
		if (x == 0 || x == farX)
			return true;

		if (y == 0 || y == farY)
			return true;

		if (z == 0 || z == farZ)
			return true;

		return false;
	}

	public bool InDeformSet(Vector3 vertex) {
		return deformSet.Contains (vertex);
	}

	public int SizeX {
		get { 
			return sizeX;
		}
	}

	public int SizeY {
		get { 
			return sizeY;
		}
	}

	public int SizeZ {
		get { 
			return sizeZ;
		}
	}

	public float Isolevel {
		get { 
			return isolevel;
		}
	}

	public float[] Isolevels {
		get { 
			return isolevels;
		}
	}

	public Generator Generator {
		get { 
			return generator;
		}
	}

	public bool[] Destructable {
		get { 
			return destructable;
		}
	}
}

