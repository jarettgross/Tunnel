using System;
using System.Collections.Generic;

using UnityEngine;

using CoherentNoise;

public class Chunk {

	public const int CHUNK_X = 10;
	public const int CHUNK_Y = 10;
	public const int CHUNK_Z = 10;

	private Vector3 position;
	private float scale;
	private GameObject chunkObject;
	private int layers;

	private List<Vector3>[] vertices;
	private List<int>[] indices;
	private List<Vector2>[] uvs;

	public Chunk(Vector3 _position, float _scale, GameObject _chunkObject, int _layers) {
		position = _position;
		scale = _scale;
		chunkObject = _chunkObject;
		layers = _layers;
		vertices = new List<Vector3>[layers];
		indices = new List<int>[layers];
		uvs = new List<Vector2>[layers];

		MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer> ();
		meshRenderer.sharedMaterials = GameObject.Find ("Constants").GetComponent<Constants> ().terrainMaterials;
	}

	public void Build(World world) {
		MeshData[] meshData = GenerateLayeredMeshData (world);
		for (int i = 0; i < layers; i++) {
			vertices[i] = meshData[i].GetVertices ();
			indices[i] = meshData[i].GetIndices ();
			uvs[i] = meshData[i].GetUVs ();
		}
	}

	public void ApplyMesh() {

		Mesh newMesh = new Mesh ();
		newMesh.subMeshCount = layers;


		List<Vector3> _vertices = new List<Vector3> ();
		List<int>[] _indices = new List<int>[layers];
		List<Vector2> _uvs = new List<Vector2> ();

		// combine vertices and uvs
		for (int i = 0; i < layers; i++) {
			_vertices.AddRange (vertices [i]);
			_uvs.AddRange (uvs [i]);
		}

		// recalculate indices
		int offset = 0;
		int size;
		for (int i = 0; i < layers; i++) {
			_indices [i] = new List<int> ();
			size = indices [i].Count;
			for (int j = 0; j < size; j++)
				_indices [i].Add (indices [i] [j] + offset);
			offset += size;
		}

		// update mesh vertices and uvs
		newMesh.vertices = _vertices.ToArray ();
		newMesh.uv = _uvs.ToArray ();

		// update submesh indices
		for (int i = 0; i < layers; i++) {
			newMesh.SetTriangles (_indices [i], i);
		}

		// update with recalculated indices
		// indices = _indices;

		newMesh.RecalculateNormals ();
		newMesh.RecalculateBounds ();

		chunkObject.GetComponent<MeshFilter> ().mesh = newMesh;
		chunkObject.GetComponent<MeshCollider> ().sharedMesh = newMesh;
	} 

	public void Deform(World world, int minX, int maxX, int minY, int maxY, int minZ, int maxZ) {

		// calculate bounds
		int startX = (int) Math.Max (minX, position.x);
		int endX = (int) Math.Min (maxX, position.x + Chunk.CHUNK_X);

		int startY = (int) Math.Max (minY, position.y);
		int endY = (int) Math.Min (maxY, position.y + Chunk.CHUNK_Y);

		int startZ = (int) Math.Max (minZ, position.z);
		int endZ = (int) Math.Min (maxZ, position.z + Chunk.CHUNK_Z);


		List<Vector3>[] containedVertices = new List<Vector3>[layers];
		List<int>[] containedIndices = new List<int>[layers];
		List<Vector2>[] containedUVs = new List<Vector2>[layers];

		for (int layer = 0; layer < layers; layer++) {

			containedVertices [layer] = new List<Vector3> ();
			containedIndices [layer] = new List<int> ();
			containedUVs [layer] = new List<Vector2> ();

			int count = 0;
			int size = vertices [layer].Count;
			for (int i = 0; i < size; i += 3) {

				if (PointWithinArea (vertices [layer] [i], startX, endX, startY, endY, startZ, endZ)) {
					if (PointWithinArea (vertices [layer] [i + 1], startX, endX, startY, endY, startZ, endZ)) {
						if (PointWithinArea (vertices [layer] [i + 2], startX, endX, startY, endY, startZ, endZ)) {
							continue;
						}
					}
				}

				for (int j = 0; j < 3; j++) {
					containedVertices [layer].Add (vertices [layer] [i + j]);
					containedIndices [layer].Add (count + MarchingCubes.windingOrder [j]);
					containedUVs [layer].Add (uvs [layer] [i + j]);
				}
				count += 3;
			}
		}
			
		MeshData[] meshData = GenerateLayeredMeshData (world, startX, endX, startY, endY, startZ, endZ);

		for (int layer = 0; layer < layers; layer++) {
			containedVertices[layer].AddRange (meshData[layer].GetVertices ());
			containedUVs[layer].AddRange (meshData[layer].GetUVs ());

			int offset = containedIndices[layer].Count;
			List<int> _indices = meshData [layer].GetIndices ();
			for (int i = 0; i < meshData[layer].GetNumVertices (); i++) {
					containedIndices[layer].Add (offset + _indices [i]);
			}

			vertices[layer] = containedVertices [layer];
			indices[layer] = containedIndices[layer];
			uvs[layer] = containedUVs [layer];
		}
	}

	private void UpdateMeshData(World world, int layer, int startX, int endX, int startY, int endY, int startZ, int endZ) {
		// examine current veritices

	}

	public GameObject ChunkObject {
		get { 
			return chunkObject;
		}
	}

	private bool PointWithinArea(Vector3 vertex, int minX, int maxX, int minY, int maxY, int minZ, int maxZ) {
		if (vertex.x < minX || vertex.x > maxX)
			return false;

		if (vertex.y < minY || vertex.y > maxY)
			return false;

		if (vertex.z < minZ || vertex.z > maxZ)
			return false;

		return true;
	}

	private MeshData[] GenerateLayeredMeshData(World world) {
		int startX = (int) position.x;
		int startY = (int) position.y;
		int startZ = (int) position.z;

		return GenerateLayeredMeshData (world, startX, startX + Chunk.CHUNK_X, startY, startY + Chunk.CHUNK_Y, startZ, startZ + Chunk.CHUNK_Z);
	}

	private MeshData[] GenerateLayeredMeshData(World world, int minX, int maxX, int minY, int maxY, int minZ, int maxZ) {

		float[] isolevels = world.Isolevels;
		MeshData[] meshData = new MeshData[isolevels.Length];
		for (int i = 0; i < isolevels.Length; i++) {
			meshData [i] = new MeshData ();
		}

		for (int x = minX; x < maxX; x++) {
			for (int y = minY; y < maxY; y++) {
				for (int z = minZ; z < maxZ; z++) {

					int xLoc = (int) (x * scale);
					int yLoc = (int) (y * scale);
					int zLoc = (int) (z * scale);

					MarchingCubes.Polygonize (xLoc, yLoc, zLoc, meshData, isolevels, world);
				}
			}
		}

		return meshData;
	}

}
