using UnityEngine;
using UnityEngine.Networking;

public class PerformAttack : NetworkBehaviour
{

    //(temporary solution) need to be changed when adding new weapons
    public float range = 100.0f;
    public float cooldown = 0.3f;
    public float damage = 50f;

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

    [SerializeField]    //use this to set the gun camera
    private GameObject weapon;

    [SerializeField]   //use this camera to guide the shooting direction
    public Camera cam;

    [SerializeField]
    private LayerMask mask;


    // Use this for initialization
    void Start () {
        // set the layer of current weapon to "Guns"
        if (isLocalPlayer)
        {
            weapon.layer = LayerMask.NameToLayer("Guns");
            foreach (Transform child in weapon.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Guns");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
       
        //if fire click detected and cooldown finished, perform attack
        if (Input.GetMouseButtonDown(0) && Time.time >(cooldown + cooldownRemaining))
        {
            CmdFire();
            cooldownRemaining = Time.time;
        }

        /*if (Input.GetMouseButtonDown(0))
        {
            if(!IsInvoking("CmdFire"))
                InvokeRepeating("CmdFire", 0f, cooldown);
        } else if(Input.GetMouseButtonUp(0))
        {
            //CancelInvoke("CmdFire");
        }*/
    }

    [Command]
    void CmdFire()
    {
        //(temporary)Play the shooting sound
        GetComponent<AudioSource>().PlayOneShot(pistol_sound);

        //use ray to simulate shooting
        Ray ray = new Ray(cam.transform.position + cam.transform.forward * 0.5f, cam.transform.forward);

        //do sth. when hit an object
        if (Physics.Raycast(ray, out hitInfo, range, mask))
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
                h.ReceiveDamage(damage);
            }

            //generate a small ball to simulate bullet
            if (debrisPrefab != null)
            {
                Instantiate(debrisPrefab, hitPoint, Quaternion.identity);
            }

            //NetworkServer.Spawn(tempBulletHole);

            
        }
    }
}
