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

	private Vector3[] vertices;
	private int[] indices;
	private Vector2[] uvs;

	private Mesh mesh;

	public Chunk(Vector3 _position, float _scale, GameObject _chunkObject) {
		position = _position;
		scale = _scale;
		chunkObject = _chunkObject;
	}

	public void Build(World world) {
		MeshData meshData = GenerateMeshData (world);
		vertices = meshData.GetVertices ();
		indices = meshData.GetIndices ();
		uvs = meshData.GetUVs ();

	}

	public void ApplyMesh() {
		Mesh mesh = ToMesh ();
		chunkObject.GetComponent<MeshFilter> ().mesh = mesh;
		chunkObject.GetComponent<MeshCollider> ().sharedMesh = mesh;
	} 

	public void Deform(World world, int minX, int maxX, int minY, int maxY, int minZ, int maxZ) {

		// calculate bounds
		int startX = (int) Math.Max (minX, position.x);
		int endX = (int) Math.Min (maxX, position.x + Chunk.CHUNK_X);

		int startY = (int) Math.Max (minY, position.y);
		int endY = (int) Math.Min (maxY, position.y + Chunk.CHUNK_Y);

		int startZ = (int) Math.Max (minZ, position.z);
		int endZ = (int) Math.Min (maxZ, position.z + Chunk.CHUNK_Z);

		// examine current veritices
		List<Vector3> containedVertices = new List<Vector3> ();
		List<int> containedIndices = new List<int> ();
		List<Vector2> containedUVs = new List<Vector2> ();
		int count = 0;
		for (int i = 0; i < vertices.Length; i += 3) {
			
			if (PointWithinArea (vertices [i], startX, endX, startY, endY, startZ, endZ)) {
				if (PointWithinArea (vertices [i + 1], startX, endX, startY, endY, startZ, endZ)) {
					if (PointWithinArea (vertices [i + 2], startX, endX, startY, endY, startZ, endZ)) {
						continue;
					}
				}
			}

			for (int j = 0; j < 3; j++) {
				containedVertices.Add (vertices[i + j]);
				containedIndices.Add(count + MarchingCubes.windingOrder[j]);
				containedUVs.Add (uvs[i + j]);
			}
			count += 3;
		}

		MeshData meshData = GenerateMeshData (world, startX, endX, startY, endY, startZ, endZ);
		containedVertices.AddRange (meshData.GetVertices ());
		containedUVs.AddRange (meshData.GetUVs ());

		int index = containedIndices.Count;
		for (int i = 0; i < meshData.GetNumVertices(); i += 3) {
			for (int j = 0; j < 3; ++j) {
				containedIndices.Add (index + MarchingCubes.windingOrder [j]);
			}
			index += 3;
		}

		vertices = containedVertices.ToArray ();
		indices = containedIndices.ToArray ();
		uvs = containedUVs.ToArray();
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

	private MeshData GenerateMeshData(World world) {
		int startX = (int) position.x;
		int startY = (int) position.y;
		int startZ = (int) position.z;

		return GenerateMeshData (world, startX, startX + Chunk.CHUNK_X, startY, startY + Chunk.CHUNK_Y, startZ, startZ + Chunk.CHUNK_Z);
	}

	private MeshData GenerateMeshData(World world, int minX, int maxX, int minY, int maxY, int minZ, int maxZ) {

		MeshData meshData = new MeshData ();

		for (int x = minX; x < maxX; x++) {
			for (int y = minY; y < maxY; y++) {
				for (int z = minZ; z < maxZ; z++) {

					int xLoc = (int) (x * scale);
					int yLoc = (int) (y * scale);
					int zLoc = (int) (z * scale);

					MarchingCubes.Polygonize (xLoc, yLoc, zLoc, meshData, world);
				}
			}
		}

		return meshData;
	}

	private Mesh ToMesh() {

		Mesh mesh = new Mesh();

		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();

		return mesh;
	}
}
