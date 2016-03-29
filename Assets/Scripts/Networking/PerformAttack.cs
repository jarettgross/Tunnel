using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PerformAttack : NetworkBehaviour
{

    //(temporary solution) need to be changed when adding new weapons
    /*public float range = 100.0f;
    public float cooldown = 0.3f;
    public float damage = 50f;*/

    public GameObject bulletHole;
    public GameObject bloodHole;
    public GameObject debrisPrefab;

    public AudioClip pistol_sound;
    
    //store info of the obeject being hit
    private RaycastHit hitInfo;

    //store the type of bulletholes 
    private GameObject tempBulletHole;

    //(temporary solution)the remaining time before able to make the next shot
    private float cooldownRemaining = 0;

    //the weapon currently equiped
    private PlayerWeapon currentWeapon;

    private WeaponManager weaponManager;

    /*[SerializeField]    //use this to set the gun camera(this is now done in WeaponManager)
    private GameObject weapon;*/

    [SerializeField]   //use this camera to guide the shooting direction
    public Camera cam;

    [SerializeField]
    private LayerMask mask;


    // Use this for initialization
    void Start () {
        weaponManager = GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();
       
        if (currentWeapon.is_automatic == true) //first check if it is an automatic weapon
        {
            if (Input.GetMouseButtonDown(0))
            {
                InvokeRepeating("Shoot", 0f, currentWeapon.cooldown); //automatic weapon will repeat shooting according firerate
            }
            else if (Input.GetMouseButtonUp(0))
            {
                CancelInvoke("Shoot");  //stop invoking Shoot
            }
        } //else perform shooting for non automatic weapon
        else if (Input.GetMouseButtonDown(0) && Time.time > (currentWeapon.cooldown + cooldownRemaining))
        {
            Shoot();
            cooldownRemaining = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))       //Press 2 to switch to the secondary weapon(for now, AK47)
            switchToSecondaryWeapon();
        else if (Input.GetKeyDown(KeyCode.Alpha1))  //Press 1 to switch to the primary weapon(for now, the pistol)
            switchToPrimaryWeapon();
    }

    //Gun switching logic: [Client] -> [Command] -> [ClientRpc] (not sure why should we do this, but it works)
    [Client]
    void switchToPrimaryWeapon()
    {
        if (!isLocalPlayer)     //need to make sure only the local player switch the weapon
        {
            return;
        }
        CmdSwitchTo1();
    }

    [Client]
    void switchToSecondaryWeapon()
    {
        if (!isLocalPlayer)     //need to make sure only the local player switch the weapon
        {
            return;
        }
        CmdSwitchTo2();
    }

    [Command]
    void CmdSwitchTo1()
    {
        RpcSwitchTo1();
    }

    [Command]
    void CmdSwitchTo2()
    {
        RpcSwitchTo2();
    }

    [ClientRpc]
    void RpcSwitchTo1()
    {
        weaponManager.SwitchToPrimaryWeapon();  //call the weapon switching method in WeaponManager
    }

    [ClientRpc]
    void RpcSwitchTo2()
    {
        weaponManager.SwitchToSecondaryWeapon();    //call the weapon switching method in WeaponManager
    }

    [Client]
    void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        CmdFire();
    }

    [Command]
    void CmdFire()
    {
        //Show muzzle flash
        RpcDoShootEffect();

        //(temporary)Play the shooting sound
        GetComponent<AudioSource>().PlayOneShot(pistol_sound);

        //use ray to simulate shooting
        Ray ray = new Ray(cam.transform.position + cam.transform.forward * 0.5f, cam.transform.forward);

        //do sth. when hit an object
        if (Physics.Raycast(ray, out hitInfo, currentWeapon.range, mask))
        {
            Vector3 hitPoint = hitInfo.point;
            
            //get the object being hit
            GameObject go = hitInfo.collider.gameObject;


            //if (go.tag == "Player")
            //    tempBulletHole = (GameObject)Instantiate(bloodHole, hitPoint, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            //else
            //    tempBulletHole = (GameObject)Instantiate(bulletHole, hitPoint, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            //tempBulletHole.transform.parent = go.transform;
            //Debug.Log("BulletHole's Parent: " + tempBulletHole.transform.parent.name);
            //Debug.Log("We hit " + hitInfo.collider.name);

            //bullethole generater
            if (go.tag == "Unbreakable")
            {
                tempBulletHole = (GameObject)Instantiate(bulletHole, hitPoint, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
                tempBulletHole.transform.parent = go.transform;
                NetworkServer.Spawn(tempBulletHole);
            }

            //add damage to the object
            HasHealth h = go.GetComponent<HasHealth>();
            if (h != null)
            {
                h.ReceiveDamage(currentWeapon.damage);
            }

            //generate a small ball to simulate bullet
            if (debrisPrefab != null)
            {
                Instantiate(debrisPrefab, hitPoint, Quaternion.identity);
            }

            //NetworkServer.Spawn(tempBulletHole);
        
        }
    }

    [ClientRpc]
    void RpcDoShootEffect() {
		if (!isLocalPlayer)
			return;

        weaponManager.GetCurrentGraphics().muzzleFlash.Play();  //active the muzzleFlash
    }
}
