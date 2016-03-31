using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class WeaponController : NetworkBehaviour {

	// Temporary starting weapon
	public GameObject tempStarterWeapon;

	// Temporary secondary weapon
	public GameObject tempSecondaryWeapon;

	/* * * * * * * * * * * * * * * * * * * * */


	// Camera for ray casting
	public Camera weaponView;

	// Layer representing local player
	public LayerMask selfLayer;

	// Layer for guns
    private string weaponLayerName = "Guns";

	[SerializeField] // Maximum number of weapons allowed
	private const int MAX_WEAPONS = 2; 

    [SerializeField] // Where new weapon should spawn
    private Transform weaponHolder;

	// Currently active weapon slot
	private int currentWeaponSlot;

	// Array of possible weapons
	private List<WeaponBase> weapons;

	// Currently active weapon
	private WeaponBase currentWeapon;

	/*
	 * Initialize weapon fields
	 */ 
	void Start() {
		weapons = new List<WeaponBase>();
		currentWeaponSlot = -1;
		currentWeapon = null;

		// Flip bits in selfLayer to exclude local player
		selfLayer = ~selfLayer;
    }

	/*
	 * Assign the starting weapons of the player
	 */ 
	public void AssignStartingWeapons() {
		
		// Temporary
		WeaponBase starterWeapon = ((GameObject)Instantiate(tempStarterWeapon, Vector3.zero, Quaternion.identity)).GetComponent<WeaponBase>();
		WeaponBase secondaryWeapon = ((GameObject)Instantiate(tempSecondaryWeapon, Vector3.zero, Quaternion.identity)).GetComponent<WeaponBase>();
		
		// Add weapons to weapon list
		AddWeapon(starterWeapon);
		AddWeapon(secondaryWeapon);

		// Equip the default weapon
		EquipWeapon(0);
	}

    /*
     * Returns the player's active weapon
     */ 
	public WeaponBase GetCurrentWeapon() {
		return currentWeapon;
    }

	/*
	 * Add a weapon to player's weapon list
	 */ 
	public bool AddWeapon(WeaponBase weapon) {
		if (weapons.Count >= MAX_WEAPONS)
			return false;

		// Attach weapon to weaponHolder
		weapon.transform.SetParent(weaponHolder, false);
		weapons.Add(weapon);

		return true;
	}

	/*
	 * Process weapon actions
	 */ 
	public void Process() {

		if (currentWeapon == null)
			return;

		if (GetCurrentWeapon().IsAutomatic) { // Automatic weapon
			if (Input.GetMouseButtonDown(0)) {
				// Automatic weapon will repeat shooting according firerate
				InvokeRepeating("Shoot", 0f, GetCurrentWeapon().Cooldown); 
			} else if (Input.GetMouseButtonUp(0)) {
				// Stop invoking Shoot
				CancelInvoke("Shoot");  
			}
		} else { // Non-automatic weapon
			if (Input.GetMouseButtonDown(0)) {
				if (Input.GetMouseButtonDown(0) && GetCurrentWeapon().Ready()) {
					Shoot();
				}
			}
		}

		float delta = 0;
		if ((delta = Input.GetAxis("Mouse ScrollWheel")) != 0) {
			Debug.Log("Delta: " + delta);
			SwitchWeapon(delta);
		}
	}

    /*
     * Equip a weapon based on weapon slot
     */ 
	private void EquipWeapon(int weaponSlot) {

		if (weaponSlot < 0 || weaponSlot >= weapons.Count) {
			Debug.LogError("Weapon slot out of bounds");
			return;
		}

		// Unequip old weapon
		if (currentWeapon != null) {
			currentWeapon.Unequip();
		}

		// Update current weapon
		currentWeapon = weapons[weaponSlot];
		currentWeaponSlot = weaponSlot;

		// Equip new weapon
		currentWeapon.Equip();
    }


	private void SwitchWeapon(float delta) {
		int newWeaponSlot = currentWeaponSlot;

		Debug.Log("Current Slot: " + currentWeaponSlot);

		if (delta > 0) {
			newWeaponSlot = currentWeaponSlot + 1;
			if (newWeaponSlot >= weapons.Count)
				newWeaponSlot = 0;
		} else {
			newWeaponSlot = currentWeaponSlot - 1;
			if (newWeaponSlot < 0)
				newWeaponSlot = weapons.Count - 1;
		}

		Debug.Log("New Weapon Slot: " + newWeaponSlot);

		if (newWeaponSlot != currentWeaponSlot) {
			Debug.Log("Equipping weapon slot: " + newWeaponSlot);
			EquipWeapon(newWeaponSlot);
		}
	}

	/*
	 * Perform client side shoot call to server
	 */ 
	[Client]
	private void Shoot() {
		if (!isLocalPlayer)
			return;

		// Get camera transform
		Transform transform = weaponView.transform;

		Debug.Log(transform.position);
		Debug.Log(transform.forward);

		// Perform ray casting
		RaycastHit hit;

		Ray ray = new Ray (transform.position, transform.forward);

		if (Physics.Raycast(ray, out hit, 100f, selfLayer)) {
			Debug.Log ("Hit: " + hit.collider.gameObject);

			// Grab the position of the hit
			Vector3 hitPosition = hit.point;

			// Grab the GameObject collider
			GameObject collider = hit.collider.gameObject;

			HasHealth health = collider.GetComponent<HasHealth>();
			if (health != null) {
				CmdHandleShot(collider);
			}

			Debug.Log(hitPosition);

			// Send deformation to server
			GetComponent<TerrainController>().CmdDeform(hitPosition);
		}

		// Reset last fire time
		GetCurrentWeapon().ResetFireTime();
	}

	/*
	 * Perform server side shot calculation
	 */ 
	[Command]
	private void CmdHandleShot(GameObject target) {

		Debug.Log("Shooting");

		// Apply damage to player
		target.GetComponent<HasHealth>().ReceiveDamage(GetCurrentWeapon().Damage);

		// Play muzzle flash
		RpcMuzzleFlash();

		//Play shooting sound
		GetComponent<AudioSource>().PlayOneShot(GetCurrentWeapon().sound);
	}

	/*
	 * Displays weapon muzzle flash
	 */ 
	[ClientRpc]
	private void RpcMuzzleFlash() {
		GetCurrentWeapon().MuzzleFlash.Play();
	}
}