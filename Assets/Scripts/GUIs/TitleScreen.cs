using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {

	public bool showInstructions = false;
	public Texture blackBackground;
	public Texture instructions;

	void OnGUI() {
		if (showInstructions) {
			Rect rect = new Rect(0, 0, Screen.width, Screen.height);
			GUI.DrawTexture (rect, blackBackground, ScaleMode.StretchToFill);
			GUI.DrawTexture (rect, instructions, ScaleMode.ScaleToFit);
			if (Input.GetMouseButtonDown (0)) {
				if (rect.Contains (Event.current.mousePosition)) {
					SceneManager.LoadScene ("Loader Scene");
				}
			}
		}
	}

	public void onInstructionClick() {
		showInstructions = true;
	}

	public void onPlayClick() {
		SceneManager.LoadScene ("Loader Scene");
	}
}
