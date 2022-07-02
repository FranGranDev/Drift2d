using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sigh : MonoBehaviour
{
    public GameObject Player;
    

    private void Awake()
    {
        if (Player == null)
        {
            Player = GameObject.Find("Car");
        }
        
    }
    private void FixedUpdate()
    {
        transform.up = (transform.position - Player.transform.position).normalized;
    }
}
