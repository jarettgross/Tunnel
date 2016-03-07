using UnityEngine;
using System.Collections;

public class BulletHoleSelfDestroy : MonoBehaviour
{

    public float duration = 1f;

    // Update is called once per frame
    void Update()
    {
        
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            Destroy(gameObject);
        }
    }
}
