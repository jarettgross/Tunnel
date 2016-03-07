using System;
using System.Collections.Generic;

using UnityEngine;

public class Deformation {

	public enum DeformationType {
		Point,
		Cube,
		Sphere
	}

	private Vector3 position;
	private DeformationType deformationType;
	private int radius;

	public Deformation(Vector3 _position) : this(_position, DeformationType.Point, 0) { }

	public Deformation(Vector3 _position, DeformationType _type) : this(_position, _type, 1) { }

	public Deformation(Vector3 _position, DeformationType _type, int _radius) {
		position = _position;
		deformationType = _type;
		radius = _radius;
	}

	public Vector3 Position {
		get { 
			return position;
		}
	}

	public int Radius {
		get { 
			return radius;
		}
	}

	public DeformationType GetDeformationType() {
			return deformationType;
	}

	public List<Vector3> Apply() {
		switch (deformationType) {
		case DeformationType.Point:
			return DeformPoint ();

		case DeformationType.Cube:
			return DeformCube ();

		case DeformationType.Sphere:
			return DeformSphere ();
		}

		return null;
	}

	private List<Vector3> DeformPoint() {
		List<Vector3> vertices = new List<Vector3> ();
		vertices.Add (position);
		return vertices;
	}

	private List<Vector3> DeformCube() {
		List<Vector3> vertices = new List<Vector3> ();
		int startX = (int)position.x;
		int startY = (int)position.y;
		int startZ = (int)position.z;
		for (int x = startX - radius; x <= startX + radius; x++) {
			for (int y = startY - radius; y <= startY + radius; y++) {
				for (int z = startZ - radius; z <= startZ + radius; z++) {
					vertices.Add (new Vector3 (x, y, z));
				}
			}
		}

		return vertices;
	}

	private List<Vector3> DeformSphere() {
		return null; // TODO
	}

}
