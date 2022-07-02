using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Damage;
    public GameObject Effect;
    public Transform Owner;
    public AudioClip Sound;
    private void Start()
    {
        StartCoroutine(Destroy());
    }
    private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(10f);
        Instantiate(Effect, transform.position, transform.rotation, null);
        GameObject Audio = Instantiate(new GameObject("Sound"), transform.position, transform.rotation, null);
        AudioSource Source = Audio.AddComponent<AudioSource>();
        Source.clip = Sound;
        Source.spatialBlend = 1f;
        Destroy(Audio, Sound.length);
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.root != Owner)
        {
            Transform car = collision.transform.root;
            if (car.tag == "Car")
            {
                car.GetComponent<Car>().GetHit(Damage);
                car.GetComponent<Rigidbody2D>().velocity *= 0.75f;
                car.GetComponent<Rigidbody2D>().AddForce(transform.up * transform.GetComponent<Rigidbody2D>().velocity.magnitude * 25f);
            }
            Instantiate(Effect, transform.position, transform.rotation, null);
            GameObject Audio = Instantiate(new GameObject("Sound"), transform.position, transform.rotation, null);
            AudioSource Source = Audio.AddComponent<AudioSource>();
            Source.clip = Sound;
            Source.spatialBlend = 1f;
            Destroy(Audio, Sound.length);
            Destroy(gameObject);
        }
    }
}

