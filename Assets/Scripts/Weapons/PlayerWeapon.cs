using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "USP";

    public bool is_automatic = false;

    public float damage = 50f;

    public float range = 100f;

    public float cooldown = 0.3f;

    public GameObject graphics;

}
