using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour
{
    public Camera Cam;
    public GameObject DropPref;
    [Range(0, 10)]
    public int Power;
    private float Height;
    private float Width;
    private Coroutine Cour;

    private void Start()
    {

    }

    public void StartRain()
    {
        Cour = StartCoroutine(Rain());
    }
    public void StopRain()
    {
        if (Cour != null)
            StopCoroutine(Cour);
    }

    private IEnumerator Rain()
    {
        while(true)
        {
            Height = 2f * Cam.orthographicSize;
            Width = Height * Cam.aspect;
            for (int i = 0; i < Power * 5; i++)
            {
                Vector3 Position = transform.position +
                new Vector3(Random.Range(-Width, Width),
                 Random.Range(-Height, Height), -Cam.transform.position.z / 2f);
                GameObject Drop = Instantiate(DropPref, Position, Quaternion.identity, null);
                Drop.GetComponent<Rigidbody2D>().AddForce(-transform.up * 500f);
                float time = Random.Range(0.5f, 2f);
                Drop.transform.localScale = new Vector3(time * Power / 2, time * Power / 2, time);
                Drop.GetComponent<Drop>().Target = Cam.transform;
                Drop.GetComponent<Drop>().OnStart(time);
            }
            yield return new WaitForFixedUpdate();
        }
    }

}
