using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TerrainManager))]
public class TerrainEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		TerrainManager renderer = (TerrainManager)target;
		if (GUILayout.Button ("Generate")) {
			renderer.Redraw ();
		}
	}
}