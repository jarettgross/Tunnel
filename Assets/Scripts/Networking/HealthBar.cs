using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthBar : NetworkBehaviour {
    private PlayerGUI hasHealth;
    public GameObject healthSliderPreFab;

    private GameObject healthSlider;
    private Slider slider;

	public void Initialize() {
		if (!isLocalPlayer) {
			return;
		}

		hasHealth = GetComponent<PlayerGUI>();
		healthSlider = Instantiate(healthSliderPreFab);
		healthSlider.transform.SetParent (transform.parent);
		slider = healthSlider.GetComponentInChildren<Slider>();
	}

    void Update() {
        if (!isLocalPlayer) {
            return;
        }
		slider.value = hasHealth.HealthRatio();
    }

    void OnDestroy() {
        if (!isLocalPlayer) {
            return;
        }
        Destroy(healthSlider);
    }
}