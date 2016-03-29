using UnityEngine;
using UnityEngine.Networking;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private string weaponLayerName = "Guns";

    [SerializeField] //the default weapon
    private PlayerWeapon primaryWeapon;

    [SerializeField] //the secondary weapon(pre-defined, temporary solution)
    private PlayerWeapon secondaryWeapon;

    [SerializeField] //where new weapon should spawn
    private Transform weaponHolder;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;

    void Start()
    {
        //equip the default weapon
        EquipWeapon(primaryWeapon); 
    }

    //return the current weapon, this method is called in the update method in PerformAttack
    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    //switch to primaryWeapon
    public void SwitchToPrimaryWeapon()
    {
        EquipWeapon(primaryWeapon);
    }

    //switch to secondaryWeapon
    public void SwitchToSecondaryWeapon()
    {
        EquipWeapon(secondaryWeapon);
    }

    //the graphic is the model of the weapon
    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    //equip the weapon according to player's input
    void EquipWeapon(PlayerWeapon _weapon)
    {
        //destroy the current equiped weapon first(maybe create a new method later)
        foreach (Transform child in weaponHolder)
        {
            if (child == null)
                continue;
            Destroy(child.gameObject);
        }

        //equip the weapon
        currentWeapon = _weapon;

        //generate the weapon graphics
        GameObject weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation * _weapon.graphics.transform.rotation);
        weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = weaponIns.GetComponent<WeaponGraphics>();
        if (currentGraphics == null)
            Debug.LogError("No weaponGraphics!");

        // set the layer of current weapon to "Guns"
        if (isLocalPlayer)
        {
            weaponIns.layer = LayerMask.NameToLayer(weaponLayerName);
            foreach (Transform child in weaponIns.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer(weaponLayerName);
            }
        }
    }
}