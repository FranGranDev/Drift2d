using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Range(0, 1000)]
    public int BulletDamage;
    [Range(500, 10000)]
    public float BulletSpeed;
    [Range(0,1)]
    public float TowerSpeed;
    [Range(0, 10)]
    public float ReloadTime;
    public Transform BulletStart;
    public GameObject Effect;
    public GameObject Bullet;

    public void Fire()
    {
        GameObject NowBullet = Instantiate(Bullet, BulletStart.position, transform.rotation, null);
        NowBullet.GetComponent<Rigidbody2D>().AddForce(transform.up * BulletSpeed);
        NowBullet.GetComponent<Bullet>().Damage = BulletDamage;
        NowBullet.GetComponent<Bullet>().Owner = transform.root;
        Instantiate(Effect, BulletStart.position, transform.rotation, transform);
    }
}
