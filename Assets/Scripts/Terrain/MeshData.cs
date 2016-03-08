using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshData {

	private List<Vector3> vertices;
	private List<int> indices;
	private List<Vector2> uvs;

	public MeshData() {
		vertices = new List<Vector3> ();
		indices = new List<int> ();
		uvs = new List<Vector2> ();
	}

	public void AddVertex(Vector3 vertex) {
		vertices.Add (vertex);
	}

	public void AddIndex(int index) {
		indices.Add (index);
	}

	public void AddUVs(Vector2 uv) {
		uvs.Add (uv);
	}

	public int GetNumVertices() {
		return vertices.Count;
	}

	public List<Vector3> GetVertices() {
		return vertices;
	}

	public List<int> GetIndices() {
		return indices;
	}

	public List<Vector2> GetUVs() {
		return uvs;
	}

}
