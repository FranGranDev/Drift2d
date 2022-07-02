using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour
{
    public Transform Target;
    public GameObject Particle;

    private Rigidbody2D Rig;
    

    private void Start()
    {
        Rig = GetComponent<Rigidbody2D>();  
    }

    public void OnStart(float time)
    {
        Destroy(gameObject, time);
    }

    public void FixedUpdate()
    {
        transform.up = Target.transform.up;
        Rig.velocity = -Target.transform.up * 10f;
    }
}
