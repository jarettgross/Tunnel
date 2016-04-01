using UnityEngine;
using System.Collections;

public class SniperRifle : WeaponBase {

	private Texture scopeCrosshair;
	public Texture[] scopeCrosshairs;
	public Camera scopeCamera;
	private Rect rectCrosshair;

	private Camera playerCamera;
	private float mainCameraFOV;
	private const float SCOPE_FOV = 15;

	private bool useScope; //using scope camera or regular camera

	void Start () {
		playerCamera = Camera.main;
		mainCameraFOV = playerCamera.fieldOfView;
		rectCrosshair = new Rect (0, 0, Screen.width, Screen.height);
		float aspectRatio = Screen.width / (float)Screen.height;
		Debug.Log (aspectRatio);
		if (aspectRatio <= 1280.0f / 1024.0f) {
			scopeCrosshair = scopeCrosshairs [0];
		} else if (aspectRatio <= 1920.0f / 1200.0f) {
			scopeCrosshair = scopeCrosshairs [1];
		} else {
			scopeCrosshair = scopeCrosshairs [2]; //1920 x 1080
		}
	}

	void OnGUI() {
		if (useScope) {
			//Set coordinates to place texture at (middle of screen)
			rectCrosshair.x = (Screen.width - rectCrosshair.width) / 2;
			rectCrosshair.y = (Screen.height - rectCrosshair.height) / 2;
			playerCamera.fieldOfView = SCOPE_FOV;
			GUI.DrawTexture (rectCrosshair, scopeCrosshair);
		} else {
			playerCamera.fieldOfView = mainCameraFOV;
		}
	}

	void Update () {
		if (Input.GetMouseButtonDown(1)) {
			useScope = !useScope;
		}
	}
}
