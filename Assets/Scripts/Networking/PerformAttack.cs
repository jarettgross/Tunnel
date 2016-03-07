using UnityEngine;
using UnityEngine.Networking;

public class PerformAttack : NetworkBehaviour
{
    public float range = 100.0f;
    public float cooldown = 0.3f;

    public GameObject bulletHole;
    public GameObject bloodHole;
    public GameObject debrisPrefab;
    public float damage = 50f;
    public AudioClip pistol_sound;
    
   
    private RaycastHit hitInfo;
    private GameObject tempBulletHole;
    float cooldownRemaining = 0;
    bool allowFire = true;

    [SerializeField]
    public Camera cam;

    [SerializeField]
    private LayerMask mask;


    // Use this for initialization
    void Start () {
        // new Vector3(1.5f,)
	}

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
       
        //cooldownRemaining -= Time.deltaTime;
	    /*if (Input.GetMouseButtonDown(0) && cooldownRemaining <= 0)
        {
            CmdFire();
        }*/
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
        //cooldownRemaining = cooldown;
        GetComponent<AudioSource>().PlayOneShot(pistol_sound);
        Ray ray = new Ray(cam.transform.position + cam.transform.forward * 0.5f, cam.transform.forward);
        if (Physics.Raycast(ray, out hitInfo, range, mask))
        {
            Vector3 hitPoint = hitInfo.point;
            GameObject go = hitInfo.collider.gameObject;
            //tempBulletHole = Instantiate(bulletHole);
            //if (go.tag == "Player")
            //    tempBulletHole = (GameObject)Instantiate(bloodHole, hitPoint, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            //else
            //    tempBulletHole = (GameObject)Instantiate(bulletHole, hitPoint, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            //tempBulletHole.transform.parent = go.transform;
            //Debug.Log("BulletHole's Parent: " + tempBulletHole.transform.parent.name);
            //Debug.Log("We hit " + hitInfo.collider.name);

            HasHealth h = go.GetComponent<HasHealth>();
            if (h != null)
            {
                h.ReceiveDamage(damage);
            }
            //if (debrisPrefab != null)
            //{
            //    Instantiate(debrisPrefab, hitPoint, Quaternion.identity);
            //}

            //NetworkServer.Spawn(tempBulletHole);

            
        }
    }
}
