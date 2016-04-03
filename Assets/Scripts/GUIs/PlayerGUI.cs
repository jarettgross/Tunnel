using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerGUI : NetworkBehaviour {

	//PLAYER HEALTH INFO
	private float hitPoints;

	[SyncVar]
	private float currentHealth;

	//*****************************

	//PLAYER DAMAGE NOTICE
	public Texture damageNotice;
	private const float endDamageNoticeAlpha = 0.8f;
	private float damageNoticeTime = 0.0f;
	private float damageNoticeTimeScale = 0.5f;
	public bool isDamaged = false;
	private Rect damageNoticeRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

	//*****************************

	void Awake() {
		hitPoints = 100;
		currentHealth = hitPoints;
	}

	//Need to initialize after awake so that player can get characterClass's healthPoints value
	public void Initialize() {
		hitPoints = gameObject.GetComponent<CharacterClass> ().HealthPoints;
		currentHealth = hitPoints;
	}

	public void ReceiveDamage(float damageAmount) {
		currentHealth -= damageAmount;
		if (currentHealth <= 0) {
			Die();
		}
		Debug.Log("Took damage. Health now: " + currentHealth);
	}

	public float HealthRatio() {
		return currentHealth / hitPoints;
	}

	void Die() {
		Destroy(gameObject);
	}

	//Flash screen red on hit
	void OnGUI() {
		if (isDamaged) {
			if (GUI.color.a != endDamageNoticeAlpha) {
				float currentAlpha = GUI.color.a;
				Color tempColor = GUI.color;
				tempColor.a = Mathf.Lerp (currentAlpha, endDamageNoticeAlpha, damageNoticeTime / damageNoticeTimeScale); //change alpha
				GUI.color = tempColor;
				GUI.DrawTexture (damageNoticeRect, damageNotice);
				damageNoticeTime += Time.deltaTime;
				if (GUI.color.a == endDamageNoticeAlpha) { //once alpha is 0, done turning invisible
					damageNoticeTime = 0.0f;
				}
			} else {
				float currentAlpha = GUI.color.a;
				Color tempColor = GUI.color;
				tempColor.a = Mathf.Lerp (0.0f, currentAlpha, damageNoticeTime / damageNoticeTimeScale); //change alpha
				GUI.color = tempColor;
				GUI.DrawTexture (damageNoticeRect, damageNotice);
				damageNoticeTime += Time.deltaTime;
				if (GUI.color.a == endDamageNoticeAlpha) { //once alpha is 0, done turning invisible
					damageNoticeTime = 0.0f;
					isDamaged = false;
				}
			}
		}
	}
}