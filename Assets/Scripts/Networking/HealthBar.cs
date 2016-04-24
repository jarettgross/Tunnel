using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthBar : NetworkBehaviour {
	private PlayerGUI hasHealth;
	public GameObject canvasHUD;

	private GameObject playerHUD;
	private Slider slider;
	private Text[] textInfo;

	private Text weaponInfo;
	private Text fuelInfo;
	private Text specialAbilityInfo;
	private Text livesRemaining;

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
		textInfo = playerHUD.GetComponentsInChildren<Text> ();
		//currentWeapon = GetComponent<WeaponController>().GetCurrentWeapon().gameObject;

		foreach (Text text in textInfo) {
			if (text.name == "ClipCounter") {
				weaponInfo = text;
			}
			if (text.name == "FuelTracker") {
				fuelInfo = text;
			}
			if (text.name == "SpecialAbility") {
				specialAbilityInfo = text;
			}
			if (text.name == "Lives") {
				livesRemaining = text;
			}
		}

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
        healthRatio = hasHealth.HealthRatio();
        slider.value = healthRatio;

		if (currentWeapon != null) {
			WeaponBase wb = currentWeapon.GetComponent<WeaponBase>();
			string[] weaponName = currentWeapon.name.Split('_');
			weaponInfo.text = wb.currentClipSize + "/" + wb.ClipSize + "\n" + weaponName[0];
			fuelInfo.text = "Fuel" + "\n" + Mathf.Round(GetComponent<PlayerController>().fuelAmount) + "/" + GetComponent<PlayerController>().originalFuelAmount;
	
			string invisible = GetComponent<PlayerController>().isInvisible ? "Invisible" : "Visible";
			string cooldown = GetComponent<PlayerController>().isCooldown ? "Cooldown" : "Ready";

			if (GetComponent<CharacterClass> ().className == "Stealth") {
				specialAbilityInfo.text = invisible + "\n" + Mathf.Round (GetComponent<PlayerController> ().invisibilityRemaining) + "/20" + "\n" + cooldown;
			}

			livesRemaining.text = "Lives: " + (3 - GetComponent<PlayerGUI> ().numDeaths).ToString();
		}
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