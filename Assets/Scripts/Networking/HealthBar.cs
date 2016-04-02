using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthBar : NetworkBehaviour
{
    HasHealth hasHealth;
    public GameObject healthSliderPreFab;

    private GameObject healthSlider;
    private Slider slider;

    private const float BAR_WIDTH = 400f;
    private const float BAR_HEIGHT = 50f;
    private const float BAR_X_POS = -450f;
    private const float BAR_Y_POS = -50f;

	public void Initialize() {
		if (!isLocalPlayer) {
			return;
		}

		hasHealth = GetComponent<HasHealth>();
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