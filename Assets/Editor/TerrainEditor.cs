using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CubesRenderer))]
public class TerrainEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		CubesRenderer renderer = (CubesRenderer)target;
		if (GUILayout.Button ("Generate")) {
			renderer.Redraw ();
		}
	}
}