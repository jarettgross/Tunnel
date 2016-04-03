using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(SoundController))]
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

	// Character class
	private CharacterClass characterClass;

    // Sound Controller
    private SoundController m_SoundController;

	/*
	 * Initialize weapon fields
	 */ 
	void Start() {
		weapons = new List<WeaponBase>();
		currentWeaponSlot = -1;
		currentWeapon = null;

		// Flip bits in selfLayer to exclude local player
		selfLayer = ~selfLayer;

        m_SoundController = GetComponent<SoundController>();
    }

	/*
	 * Assign the starting weapons of the player
	 */ 
	public void Initialize() {
		characterClass = GetComponent<CharacterClass>();
        
		CmdInitialize(characterClass.DefautWeapon.gameObject, characterClass.ClassWeapon1.gameObject, characterClass.ClassWeapon2.gameObject);
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

		// Handle weapon switching with middle mouse scroll
		float delta = 0;
		if ((delta = Input.GetAxis("Mouse ScrollWheel")) != 0) {
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

		if (!isLocalPlayer)
			return;

		// Equip new weapon
		currentWeapon.Equip();
    }

	/*
	 * Switch weapons based on middle mouse scroll value
	 */ 
	private void SwitchWeapon(float delta) {
		int newWeaponSlot = currentWeaponSlot;

		if (delta > 0) {
			newWeaponSlot = currentWeaponSlot + 1;
			if (newWeaponSlot >= weapons.Count)
				newWeaponSlot = 0;
		} else {
			newWeaponSlot = currentWeaponSlot - 1;
			if (newWeaponSlot < 0)
				newWeaponSlot = weapons.Count - 1;
		}

		if (newWeaponSlot != currentWeaponSlot) {
			EquipWeapon(newWeaponSlot);
		}
	}

	[Command]
	private void CmdInitialize(GameObject _defaultWeapon, GameObject _classWeapon1, GameObject _classWeapon2) {

		// Temporary
		GameObject defaultWeapon = (GameObject)Instantiate(tempStarterWeapon, Vector3.zero, Quaternion.identity);
		GameObject classWeapon1 = (GameObject)Instantiate(tempSecondaryWeapon, Vector3.zero, Quaternion.identity);
		//GameObject classWeapon2 = (GameObject)Instantiate(_classWeapon2, Vector3.zero, Quaternion.identity);

		NetworkServer.SpawnWithClientAuthority(defaultWeapon, connectionToClient);
		NetworkServer.SpawnWithClientAuthority(classWeapon1, connectionToClient);
		//NetworkServer.SpawnWithClientAuthority(classWeapon2, connectionToClient);

		RpcInitialize(defaultWeapon, classWeapon1);
	}

	[ClientRpc]
	private void RpcInitialize(GameObject _defaultWeapon, GameObject _classWeapon1) {

		WeaponBase defaultWeapon = _defaultWeapon.GetComponent<WeaponBase>();
		WeaponBase classWeapon1 = _classWeapon1.GetComponent<WeaponBase>();
		//WeaponBase classWeapon2 = _classWeapon2.GetComponent<WeaponBase>();

//		defaultWeapon.gameObject.name = "Pistol";
//		starterWeapon.gameObject.name = "Uzi";

		// Add weapons to weapon list
		AddWeapon(defaultWeapon);
		AddWeapon(classWeapon1);
		//AddWeapon(classWeapon2);

		// Equip the default weapon
		EquipWeapon(0);
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

		if (Physics.Raycast(ray, out hit, currentWeapon.Range, selfLayer)) {
			Debug.Log ("Hit: " + hit.collider.gameObject);

			// Grab the position of the hit
			Vector3 hitPosition = hit.point;

			// Grab the GameObject collider
			GameObject collider = hit.collider.gameObject;

			HasHealth health = collider.GetComponent<HasHealth>();
			if (health != null) {
				CmdHandleShot(collider, GetCurrentWeapon().gameObject);
			}

			// Send deformation to server
			GetComponent<TerrainController>().CmdDeform(hitPosition, GetCurrentWeapon().DeformationRadius);
		}

        //Play shooting sound
        //FIXME network the sounds
        WeaponBase weaponBase = GetCurrentWeapon().gameObject.GetComponent<WeaponBase>();
        m_SoundController.PlayClip(weaponBase.sound);

        // Play weapon muzzle flash
        GetCurrentWeapon().PlayMuzzleFlash();

		// Reset last fire time
		GetCurrentWeapon().ResetFireTime();
	}

	/*
	 * Perform server side shot calculation
	 */ 
	[Command]
	private void CmdHandleShot(GameObject target, GameObject weapon) {

		Debug.Log("Shooting " + target.name);
		WeaponBase weaponBase = weapon.GetComponent<WeaponBase>();

		// Apply damage to player
		target.GetComponent<HasHealth>().ReceiveDamage(weaponBase.Damage);
	}
		
}