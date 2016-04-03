using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterClass : MonoBehaviour {

	// Default pistol
	[SerializeField]
	protected WeaponBase defaultWeapon;

	[SerializeField] // Only set this in a character class prefab
	protected WeaponBase classWeapon1;

	[SerializeField] // Only set this in a character class prefab
	protected WeaponBase classWeapon2;

	[SerializeField]
	protected float healthPoints;

	[SerializeField]
	protected float walkSpeed;

	[SerializeField]
	protected float runSpeed;

	public string className;


	public void Initialize(CharacterClass other) {
		defaultWeapon = other.defaultWeapon;
		classWeapon1 = other.classWeapon1;
		classWeapon2 = other.classWeapon2;
		healthPoints = other.healthPoints;
		walkSpeed = other.walkSpeed;
		runSpeed = other.runSpeed;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public WeaponBase DefaultWeapon {
		get {
			return defaultWeapon;
		}
	}

	public WeaponBase ClassWeapon1 {
		get { 
			return classWeapon1;
		}
	}

	public WeaponBase ClassWeapon2 {
		get {
			return classWeapon2;
		}
	}

	public float HealthPoints {
		get { 
			return healthPoints;
		}
	}

	public float WalkSpeed {
		get { 
			return walkSpeed;
		}
	}

	public float RunSpeed {
		get { 
			return runSpeed;
		}
	}
}
