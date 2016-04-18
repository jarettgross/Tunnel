using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthBar : NetworkBehaviour {
	private PlayerGUI hasHealth;
	public GameObject canvasHUD;

	private GameObject playerHUD;
	private Slider slider;
	private Text weaponInfo;

	public Text clipInfo;

	private GameObject currentWeapon;

	private Image[] canvasImages;
	private Image damageNotice;
	private bool isDamaged = false;
	private const float endDamageNoticeAlpha = 0.8f;
	private float damageNoticeTime = 0.0f;
	private float damageNoticeTimeScale = 0.4f;

	private float healthRatio;
	private bool isChangingAlphaUp = false;


	public void Initialize() {
		if (!isLocalPlayer) {
			return;
		}

		hasHealth = GetComponent<PlayerGUI>();
		playerHUD = Instantiate(canvasHUD);
		playerHUD.transform.SetParent (transform.parent);
		slider = playerHUD.GetComponentInChildren<Slider>();
		weaponInfo = playerHUD.GetComponentInChildren<Text> ();
		currentWeapon = GetComponent<WeaponController> ().tempStarterWeapon;

		canvasImages = playerHUD.GetComponentsInChildren<Image> ();
		foreach (Image image in canvasImages) {
			if (image.name == "DamageNotice") {
				damageNotice = image;
			}
		}

		GameObject[] allHUDs = GameObject.FindGameObjectsWithTag ("HUD");
		foreach (GameObject HUD in allHUDs) {
			if (HUD != playerHUD) {
				Destroy (HUD);
			}
		}

		healthRatio = hasHealth.HealthRatio ();
	}

	void Update() {
		if (!isLocalPlayer) {
			return;
		}

		if (healthRatio != hasHealth.HealthRatio()) {
			isChangingAlphaUp = true;
			isDamaged = true;
		}
		slider.value = hasHealth.HealthRatio();
		healthRatio = hasHealth.HealthRatio();

		WeaponBase wb = currentWeapon.GetComponent<WeaponBase> ();
		string[] weaponName = currentWeapon.name.Split('_');
		weaponInfo.text = wb.currentClipSize + "/" + wb.ClipSize + "\n" + weaponName[0];
	}

	public void SetCurrentWeapon(GameObject weapon) {
		currentWeapon = weapon;
	}

	void OnGUI() {
		//Flash screen red on hit
		if (isDamaged) {
			if (damageNotice.color.a != endDamageNoticeAlpha && isChangingAlphaUp) {
				Color tempColor = damageNotice.color;
				tempColor.a = Mathf.Lerp (0, endDamageNoticeAlpha, damageNoticeTime / damageNoticeTimeScale); //change alpha
				damageNotice.color = tempColor;
				damageNoticeTime += Time.deltaTime;
				if (damageNotice.color.a == endDamageNoticeAlpha) { //once alpha is 0, done turning invisible
					damageNoticeTime = 0.0f;
					isChangingAlphaUp = false;
				}
			} else {
				Color tempColor = damageNotice.color;
				tempColor.a = Mathf.Lerp (endDamageNoticeAlpha, 0, damageNoticeTime / damageNoticeTimeScale); //change alpha
				damageNotice.color = tempColor;
				damageNoticeTime += Time.deltaTime;
				if (damageNotice.color.a == 0) { //once alpha is 0, done turning invisible
					damageNoticeTime = 0.0f;
					isDamaged = false;
				}
			}
		}
	}
		
	void OnDestroy() {
		if (!isLocalPlayer) {
			return;
		}
	}
}