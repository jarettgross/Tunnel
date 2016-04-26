using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(SoundController))]
public class WeaponController : NetworkBehaviour {

//	// Temporary starting weapon
//	public GameObject tempStarterWeapon;
//
//	// Temporary secondary weapons
//	public GameObject tempSecondaryWeapon;

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
    public Transform weaponHolder = null;

	// Currently active weapon slot
	private int currentWeaponSlot;

	// Array of possible weapons
	public List<WeaponBase> weapons;

	// Currently active weapon
	private WeaponBase currentWeapon;

	// Character class
	private CharacterClass characterClass;

    // Sound Controller
    private SoundController m_SoundController;


	// If the grenade is active and thrown
	private bool isThrown = false;
	private bool preventWeaponSwitch = false;

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
//		tempStarterWeapon = characterClass.DefaultWeapon.gameObject;
//		tempSecondaryWeapon = characterClass.ClassWeapon1.gameObject;

		CmdInitialize(characterClass.classIndex);
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
		
		if (GetCurrentWeapon ().currentClipSize > 0) { //can only shoot if you have clips
			if (!preventWeaponSwitch) { //can't switch weapons or shoot while throwing a grenade
				if (GetCurrentWeapon ().IsAutomatic) { // Automatic weapon
					if (Input.GetMouseButtonDown (0)) {
						// Automatic weapon will repeat shooting according firerate
						if (GetComponent<PlayerController> ().isInvisible) {
							GetComponent<ExtraWeaponController> ().CmdInvisiblity (GetComponent<PlayerController> ().playerUniqueID, false);
						}
						GetComponent<PlayerController>().isInvisible = false;
						GetComponent<PlayerController> ().isCooldown = true;
						GetComponent<PlayerController> ().invisibleCooldown = 5.0f;

						InvokeRepeating ("Shoot", 0f, GetCurrentWeapon ().Cooldown); 
					} else if (Input.GetMouseButtonUp (0)) {
						// Stop invoking Shoot
						CancelInvoke ("Shoot");  
					}
				} else if (!isThrown && GetCurrentWeapon ().GetComponent<Grenade> () != null) { //Grenade
					if (Input.GetMouseButtonDown (0) && GetCurrentWeapon ().Ready ()) {
					if (GetComponent<PlayerController> ().isInvisible) {
							GetComponent<ExtraWeaponController> ().CmdInvisiblity (GetComponent<PlayerController> ().playerUniqueID, false);
						}
						GetComponent<PlayerController>().isInvisible = false;
						GetComponent<PlayerController> ().isCooldown = true;
						GetComponent<PlayerController> ().invisibleCooldown = 5.0f;

						StartCoroutine (ThrowGrenade ());
					}
				} else if (!GetCurrentWeapon ().IsAutomatic) { //Non-automatic weapon
					if (Input.GetMouseButtonDown (0) && GetCurrentWeapon ().Ready ()) {
						if (GetComponent<PlayerController> ().isInvisible) {
							GetComponent<ExtraWeaponController> ().CmdInvisiblity (GetComponent<PlayerController> ().playerUniqueID, false);
						}
						GetComponent<PlayerController>().isInvisible = false;
						GetComponent<PlayerController> ().isCooldown = true;
						GetComponent<PlayerController> ().invisibleCooldown = 5.0f;

						Shoot ();
					}
				}
			}
		} else {
			CancelInvoke ("Shoot");
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
		GetComponent<HealthBar> ().SetCurrentWeapon (currentWeapon.gameObject);
    }

	/*
	 * Switch weapons based on middle mouse scroll value
	 */ 
	private void SwitchWeapon(float delta) {
		if (!preventWeaponSwitch) {
			if (GetCurrentWeapon().GetComponent<SniperRifle>() != null) {
				GetCurrentWeapon ().GetComponent<SniperRifle> ().useScope = false;
			}
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
				EquipWeapon (newWeaponSlot);
			}
		}

		if (GetCurrentWeapon().GetComponent<SniperRifle>() == null) {
			foreach (Transform t in transform) {
				if (t.gameObject.name == "FirstPersonCharacter") {
					Camera scopeCamera = t.gameObject.GetComponent<Camera> ();
					if (scopeCamera != null) {
						scopeCamera.fieldOfView = 55;
					}
				}
			}
		}

	}

	//Initially deactivate other starting weapons
	private void DeactivateWeapons() {
		foreach (WeaponBase wb in weapons) {
			wb.gameObject.SetActive (false);
		}
	}

	[Command]
	private void CmdInitialize(int classIndex) {

		CharacterClass cc = GameObject.Find("Characters").GetComponent<CharacterContainer>().GetClass(classIndex).GetComponent<CharacterClass>();

		// Temporary
		GameObject defaultWeapon = (GameObject)Instantiate(cc.DefaultWeapon, Vector3.zero, Quaternion.identity);
		GameObject classWeapon1 = (GameObject)Instantiate(cc.ClassWeapon1, Vector3.zero, Quaternion.identity);
		GameObject classWeapon2 = (GameObject)Instantiate(cc.ClassWeapon2, Vector3.zero, Quaternion.identity);


		NetworkServer.SpawnWithClientAuthority(defaultWeapon, connectionToClient);
		NetworkServer.SpawnWithClientAuthority(classWeapon1, connectionToClient);
		NetworkServer.SpawnWithClientAuthority(classWeapon2, connectionToClient);

		RpcInitialize(defaultWeapon, classWeapon1, classWeapon2);
	}

	[ClientRpc]
	private void RpcInitialize(GameObject _defaultWeapon, GameObject _classWeapon1, GameObject _classWeapon2) {

		WeaponBase defaultWeapon = _defaultWeapon.GetComponent<WeaponBase>();
		WeaponBase classWeapon1 = _classWeapon1.GetComponent<WeaponBase>();
		WeaponBase classWeapon2 = _classWeapon2.GetComponent<WeaponBase>();

		// Add weapons to weapon list
		AddWeapon(defaultWeapon);
		AddWeapon(classWeapon1);
		AddWeapon(classWeapon2);

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
		GetCurrentWeapon ().currentClipSize -= 1;

		if (Physics.Raycast(ray, out hit, currentWeapon.Range, selfLayer)) {
			Debug.Log ("Hit: " + hit.collider.gameObject);

			// Grab the position of the hit
			Vector3 hitPosition = hit.point;

			// Grab the GameObject collider
			GameObject collider = hit.collider.gameObject;

			PlayerGUI health = collider.GetComponent<PlayerGUI>();
			if (health != null) {
				collider.GetComponent<PlayerController> ().PlayHitEffect (transform.forward);
				CmdHandleShot (collider, GetCurrentWeapon ().gameObject, 0);
			} else {
				// Send deformation to server
				GetComponent<TerrainController> ().CmdDeform (hitPosition, GetCurrentWeapon ().DeformationRadius);

				GetComponent<TerrainController>().CmdTerrainParticles(hitPosition, -transform.forward);
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
		preventWeaponSwitch = true;
		WeaponBase grenade = GetCurrentWeapon ();
		grenade.currentClipSize -= 1;
		if (!isLocalPlayer) {
			yield break;
		}
		isThrown = true;
		//Throw grenade
		grenade.transform.parent = null;
		Vector3 directionFacing = weaponView.transform.forward.normalized;
		Vector3 forceVector = new Vector3(directionFacing.x, directionFacing.y, directionFacing.z) * GetCurrentWeapon().GetComponent<Grenade>().throwForce;
		grenade.GetComponent<Grenade>().gameObject.AddComponent<Rigidbody> ();
		Physics.IgnoreCollision (gameObject.GetComponent<Collider> (), grenade.GetComponent<Grenade> ().GetComponent<Collider> ());
		grenade.GetComponent<Grenade>().gameObject.GetComponent<Rigidbody>().AddForce (forceVector);

		yield return new WaitForSeconds(3);

		//Check for grenade collisions
		Collider[] objectsInExplosion = Physics.OverlapSphere (grenade.GetComponent<Grenade>().gameObject.transform.position, grenade.DeformationRadius);
		foreach (Collider col in objectsInExplosion) {
			if (col.gameObject.GetComponent<PlayerGUI>() != null) {
				float distanceToBlast = (grenade.transform.position - col.gameObject.transform.position).magnitude;
				float damageDropoff = 1 - distanceToBlast / grenade.DeformationRadius; //linear dropoff
				CmdHandleShot (col.gameObject, grenade.gameObject, grenade.damage * damageDropoff);
			}
		}

		GetComponent<ExtraWeaponController> ().CmdGrenadeParticles (grenade.transform.position, Vector3.up);
		GetComponent<TerrainController> ().CmdDeform (grenade.transform.position, grenade.DeformationRadius);

		Destroy (currentWeapon.GetComponent<Grenade> ().gameObject.GetComponent<Rigidbody> ());
		grenade.transform.SetParent(weaponHolder, false);
		currentWeapon.transform.position = weaponHolder.position;
		currentWeapon.transform.rotation = weaponHolder.rotation;
		isThrown = false;
		preventWeaponSwitch = false;
	}

	/*
	 * Perform server side shot calculation
	 */ 
	[Command]
	private void CmdHandleShot(GameObject target, GameObject weapon, float damage) {

		Debug.Log("Shooting " + target.name);
		WeaponBase weaponBase = weapon.GetComponent<WeaponBase>();

		// Apply damage to player
		bool dead = false;
		if (damage == 0) {
			dead = target.GetComponent<PlayerGUI>().ReceiveDamage(weaponBase.Damage);
		} else {
			dead = target.GetComponent<PlayerGUI>().ReceiveDamage(damage);
		}

		if (dead) {
			PlayerDied(target);
		}
	}
		
	private void PlayerDied(GameObject player) {
		Debug.Log("Player " + player + " died");
		player.transform.position = new Vector3(5, 20, 5);
		CancelInvoke ("Shoot");
	}
}