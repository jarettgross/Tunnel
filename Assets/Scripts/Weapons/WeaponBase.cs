using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(NetworkIdentity))]
public class WeaponBase : NetworkBehaviour {

	public GameObject bulletHole;
	public GameObject bloodHole;
	public GameObject debrisPrefab;
	public AudioClip sound;
	public ParticleSystem muzzleFlashPrefab;

	[SerializeField] // Damage dealt by weapon
	protected float damage = 10f;

	[SerializeField] // Does weapon need to be reloaded
	protected bool needsReload = true;

	[SerializeField] // Is this an automatic weapon
	protected bool isAutomatic = false;

	[SerializeField] // Number of bullets in a clip
	protected int clipSize = 10;

	[SerializeField] // Distance from impact point at which player will be hurt by weapon blast
	protected float hitboxRadius = 0;

	[SerializeField] // Range of weapon
	protected float range = 100;

	[SerializeField] // Delay between shots
	protected float cooldown = 0.5f;

	[SerializeField] // Weapon model
	protected GameObject weaponModel;

	// Weapon muzzle flash instance
	private ParticleSystem muzzleFlash;

	// Time of last shot
	private float lastFired;

	// Instance of weapon model GameObject
	private GameObject modelInstance;

	/*
	 * Initialize private fields
	 */ 
	public void Start() {
		lastFired = 0;
		modelInstance = null;
		//muzzleFlash = ((GameObject)Instantiate(muzzleFlashPrefab.gameObject, Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
	}

	/*
	 * On equipping the weapon, instantiate the weapon model.
	 */
	public void Equip() {
		CmdEquip();
	}

	/*
	 * On unequipping the weapon, destroy the weapon model.
	 */ 
	public void Unequip() {
		CmdUnequip();
	}
		
	/*
	 * Resets fire time on weapon
	 */ 
	public void ResetFireTime() {
		lastFired = Time.time;
	}

	/*
	 * Plays this weapon's muzzle flash
	 */ 
	public void PlayMuzzleFlash() {
		CmdPlayMuzzleFlash();
	}


	/* * * * * * * * * * * * * * * *
	 * 	Equipping and Unequipping Messages
	 * * * * * * * * * * * * * * * */ 

	[Command]
	private void CmdEquip() {
		modelInstance = (GameObject) Instantiate (weaponModel);
		NetworkServer.SpawnWithClientAuthority(modelInstance, connectionToClient);

		RpcEquip(modelInstance);
	}

	[Command]
	private void CmdUnequip() {
		NetworkServer.Destroy(modelInstance);

		RpcUnequip();
	}

	[ClientRpc]
	private void RpcEquip(GameObject model) {
		modelInstance = model;

		modelInstance.transform.rotation = weaponModel.transform.rotation;
		modelInstance.transform.SetParent(gameObject.transform, false);
		//muzzleFlash.transform.position = modelInstance.transform.position;
	}

	[ClientRpc]
	private void RpcUnequip() {
		modelInstance = null;
	}
		
	[Command]
	private void CmdPlayMuzzleFlash() {
		RpcPlayMuzzleFlash();
	}

	[ClientRpc]
	private void RpcPlayMuzzleFlash() {
		//muzzleFlash.Play();
	}

	/* * * * * * * * * * * * * * * *
	 * 		Getters and Setters
	 * * * * * * * * * * * * * * * */ 

		
		/* * * * * * * * * * * * * * * *
		* 	Custom Getters and Setters
		* * * * * * * * * * * * * * * */ 


	/*
	 * Returns true if the weapon is ready to be fired
	 */
	public bool Ready() {
		return (Time.time > (cooldown + lastFired));
	}


		/* * * * * * * * * * * * * * * *
		* 		Field Getters
		* * * * * * * * * * * * * * * */ 

	public float Damage {
		get { 
			return this.damage;
		}
	}

	public bool NeedsReload {
		get { 
			return this.needsReload;
		}
	}

	public bool IsAutomatic {
		get {
			return this.isAutomatic;
		}
	}

	public int ClipSize {
		get {
			return this.clipSize;
		}
	}

	public float HitboxRadius {
		get {
			return this.hitboxRadius;
		}
	}

	public float Range {
		get {
			return this.range;
		}
	}

	public float Cooldown {
		get {
			return this.cooldown;
		}
	}

	public GameObject WeaponModel {
		get {
			return this.weaponModel;
		}
	}

	public float LastFired {
		get {
			return this.lastFired;
		}
	}

	public GameObject ModelInstance {
		get {
			return this.modelInstance;
		}
	}

	public ParticleSystem MuzzleFlash {
		get {
			return this.muzzleFlash;
		}
	}
}
