using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponController))]
public class ActionController : NetworkBehaviour {

	// The player's weapon controller
	private WeaponController weaponController;

    // Use this for initialization
	public override void OnStartLocalPlayer() {
		weaponController = GetComponent<WeaponController>();
    }

    // Update is called once per frame
	void Update() {
		if (!isLocalPlayer)
			return;
		
		// Process weapon controller
		weaponController.Process();
    }
}
