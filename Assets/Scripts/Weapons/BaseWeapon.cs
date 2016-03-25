using UnityEngine;
using System.Collections;

public class BaseWeapon : MonoBehaviour {

	protected float damageAmount;

	protected bool needsReload;
	protected int reloadAmount;

	protected float hitboxRadius;

	protected float range;
	protected float cooldown;

	public GameObject bulletHole;
	public GameObject bloodHole;
	public GameObject debrisPrefab;

	public AudioClip pistol_sound;

	//store info of the obeject being hit
	protected RaycastHit hitInfo;

	//store the type of bulletholes 
	protected GameObject tempBulletHole;

	//(temporary solution)the remaining time before able to make the next shot
	protected float cooldownRemaining = 0;

	[SerializeField]    //use this to set the gun camera
	protected GameObject weaponCamera;

	[SerializeField]   //use this camera to guide the shooting direction
	protected Camera shootingDirection;

	[SerializeField]
	protected LayerMask mask;

	public BaseWeapon(float _damageAmount, bool _needsReload, int _reloadAmount, float _hitboxRadius, float _range, float _cooldown, string weaponCameraLayer) {
		damageAmount = _damageAmount;
		needsReload = _needsReload;
		reloadAmount = _reloadAmount;
		hitboxRadius = _hitboxRadius;
		range = _range;
		cooldown = _cooldown;

		weaponCamera.layer = LayerMask.NameToLayer(weaponCameraLayer);
		foreach (Transform child in weaponCamera.transform) {
			child.gameObject.layer = LayerMask.NameToLayer(weaponCameraLayer);
		}
	}
}
