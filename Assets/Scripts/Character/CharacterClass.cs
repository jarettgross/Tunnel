using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CharacterClass : NetworkBehaviour {

	// Default pistol
	[SerializeField]
	protected GameObject defaultWeapon;

	[SerializeField] // Only set this in a character class prefab
	protected GameObject classWeapon1;

	[SerializeField] // Only set this in a character class prefab
	protected GameObject classWeapon2;

	[SerializeField]
	protected float healthPoints;

	[SerializeField]
	protected float walkSpeed;

	[SerializeField]
	protected float runSpeed;

	public string className;

	public int classIndex;


	public void Initialize(CharacterClass other) {
		defaultWeapon = other.defaultWeapon;
		classWeapon1 = other.classWeapon1;
		classWeapon2 = other.classWeapon2;
		healthPoints = other.healthPoints;
		walkSpeed = other.walkSpeed;
		runSpeed = other.runSpeed;
		className = other.className;
		classIndex = other.classIndex;
	}

	public GameObject DefaultWeapon {
		get {
			return defaultWeapon;
		}
	}

	public GameObject ClassWeapon1 {
		get { 
			return classWeapon1;
		}
	}

	public GameObject ClassWeapon2 {
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
