using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {

	public bool showInstructions = false;
	public bool showHowTo = false;

	public Texture blackBackground;
	public Texture instructions;
	public Texture howToPlay;

	void OnGUI() {
		if (showInstructions) {
			Rect rect = new Rect(0, 0, Screen.width, Screen.height);
			GUI.DrawTexture (rect, blackBackground, ScaleMode.StretchToFill);
			GUI.DrawTexture (rect, instructions, ScaleMode.ScaleToFit);
			if (Input.GetMouseButtonDown (0) && !showHowTo) {
				if (rect.Contains (Event.current.mousePosition)) {
					SceneManager.LoadScene ("Loader Scene");
				}
			}
			showHowTo = false;
		} else if (showHowTo) {
			Rect rect = new Rect(0, 0, Screen.width, Screen.height);
			GUI.DrawTexture (rect, blackBackground, ScaleMode.StretchToFill);
			GUI.DrawTexture (rect, howToPlay, ScaleMode.ScaleToFit);
			if (Input.GetMouseButtonDown (0)) {
				if (rect.Contains (Event.current.mousePosition)) {
					showInstructions = true;
				}
			}
		}
	}

	public void onInstructionClick() {
		showHowTo = true;
	}

	public void onPlayClick() {
		SceneManager.LoadScene ("Loader Scene");
	}
}
