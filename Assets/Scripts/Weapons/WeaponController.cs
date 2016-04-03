<<<<<<< HEAD
﻿using UnityEngine;
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
		
=======
﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

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
    //private string weaponLayerName = "Guns";

	[SerializeField] // Maximum number of weapons allowed
	private const int MAX_WEAPONS = 6; 

    [SerializeField] // Where new weapon should spawn
    private Transform weaponHolder = null;

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


	// If the grenade is active and thrown
	private bool isThrown = false;

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

		CmdInitialize(characterClass.DefaultWeapon.gameObject, characterClass.ClassWeapon1.gameObject, characterClass.ClassWeapon2.gameObject);
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

		if (GetCurrentWeapon ().IsAutomatic) { // Automatic weapon
			if (Input.GetMouseButtonDown (0)) {
				// Automatic weapon will repeat shooting according firerate
				InvokeRepeating ("Shoot", 0f, GetCurrentWeapon ().Cooldown); 
			} else if (Input.GetMouseButtonUp (0)) {
				// Stop invoking Shoot
				CancelInvoke ("Shoot");  
			}
		} else if (!isThrown && GetCurrentWeapon ().GetComponent<Grenade> () != null) { //Grenade
			if (Input.GetMouseButtonDown(0) && GetCurrentWeapon().Ready()) {
				StartCoroutine(ThrowGrenade());
			}
		} else if (!GetCurrentWeapon ().IsAutomatic) { //Non-automatic weapon
			if (Input.GetMouseButtonDown(0) && GetCurrentWeapon().Ready()) {
				Shoot();
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
			currentWeapon.gameObject.SetActive (false);
			currentWeapon.Unequip();
		}

		// Update current weapon
		currentWeapon = weapons[weaponSlot];
		currentWeaponSlot = weaponSlot;
		currentWeapon.gameObject.SetActive (true);

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

	//Initially deactivate other starting weapons
	private void DeactivateWeapons() {
		foreach (WeaponBase wb in weapons) {
			wb.gameObject.SetActive (false);
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

		// Add weapons to weapon list
		AddWeapon(defaultWeapon);
		AddWeapon(classWeapon1);
		//AddWeapon(classWeapon2);

		// Equip the default weapon
		DeactivateWeapons();
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

		if (Physics.Raycast(ray, out hit, 100f, selfLayer)) {
			Debug.Log ("Hit: " + hit.collider.gameObject);

			// Grab the position of the hit
			Vector3 hitPosition = hit.point;

			// Grab the GameObject collider
			GameObject collider = hit.collider.gameObject;

			PlayerGUI health = collider.GetComponent<PlayerGUI>();
			if (health != null) {
				collider.GetComponent<PlayerController> ().PlayHitEffect (transform.forward);
				CmdHandleShot (collider, GetCurrentWeapon ().gameObject);
			} else {
				// Send deformation to server
				GetComponent<TerrainController> ().CmdDeform (hitPosition, GetCurrentWeapon ().DeformationRadius);
			}
		}

        //Play shooting sound
        WeaponBase weaponBase = GetCurrentWeapon().gameObject.GetComponent<WeaponBase>();
        m_SoundController.PlayClip(weaponBase.sound);

        // Play weapon muzzle flash
        GetCurrentWeapon().PlayMuzzleFlash();

		// Reset last fire time
		GetCurrentWeapon().ResetFireTime();
	}

	[Client]
	public IEnumerator ThrowGrenade() {
		if (!isLocalPlayer) {
			yield break;
		}
		isThrown = true;
		//Throw grenade
		GetCurrentWeapon().transform.parent = null;
		Vector3 directionFacing = weaponView.transform.forward.normalized;
		Vector3 forceVector = new Vector3(directionFacing.x, Mathf.Abs(directionFacing.y * 5), directionFacing.z) * GetCurrentWeapon().GetComponent<Grenade>().throwForce;
		GetCurrentWeapon().GetComponent<Grenade>().gameObject.AddComponent<Rigidbody> ();
		Physics.IgnoreCollision (gameObject.GetComponent<Collider> (), GetCurrentWeapon ().GetComponent<Grenade> ().GetComponent<Collider> ());
		GetCurrentWeapon().GetComponent<Grenade>().gameObject.GetComponent<Rigidbody>().AddForce (forceVector);

		yield return new WaitForSeconds(3);

		//Check for grenade collisions
		Collider[] objectsInExplosion = Physics.OverlapSphere (GetCurrentWeapon().GetComponent<Grenade>().gameObject.transform.position, GetCurrentWeapon().DeformationRadius);
		foreach (Collider col in objectsInExplosion) {
			if (col.gameObject.GetComponent<PlayerGUI>() != null) {
				float distanceToBlast = (GetCurrentWeapon().transform.position - col.gameObject.transform.position).magnitude;
				float damageDropoff = 1 - distanceToBlast / GetCurrentWeapon().DeformationRadius; //linear dropoff
				GetCurrentWeapon().damage *= damageDropoff;
				CmdHandleShot (col.gameObject, GetCurrentWeapon ().gameObject);
			}
		}
		GetComponent<TerrainController> ().CmdDeform (GetCurrentWeapon().transform.position, GetCurrentWeapon ().DeformationRadius);

		Destroy (currentWeapon.GetComponent<Grenade> ().gameObject.GetComponent<Rigidbody> ());
		GetCurrentWeapon().transform.SetParent(weaponHolder, false);
		currentWeapon.transform.position = weaponHolder.position;
		currentWeapon.transform.rotation = weaponHolder.rotation;
		isThrown = false;

	}

	/*
	 * Perform server side shot calculation
	 */ 
	[Command]
	private void CmdHandleShot(GameObject target, GameObject weapon) {

		Debug.Log("Shooting " + target.name);
		WeaponBase weaponBase = weapon.GetComponent<WeaponBase>();

		// Apply damage to player
		target.GetComponent<PlayerGUI>().ReceiveDamage(weaponBase.Damage);
		target.GetComponent<PlayerGUI>().isDamaged = true;
	}
>>>>>>> db0da317f8e12a815b609c9f013ba99ba09e422d
}