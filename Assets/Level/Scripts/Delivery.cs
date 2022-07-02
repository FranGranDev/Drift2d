using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery : MonoBehaviour
{
    public GameObject[] Box;
    private GameObject NowBox;
    public Animator anim;
    public Trigger Trig;
    public void MakeBox()
    {
        StartCoroutine(MakingBox());
    }
    IEnumerator MakingBox()
    {
        anim.SetTrigger("ToDelivered");
        yield return new WaitForSeconds(0.5f);
        NowBox = Instantiate(Box[Random.Range(0, 4)], transform.position, transform.rotation, null);
        yield break;
    }
    public void Restart()
    {
        anim.SetTrigger("ToIdle");
        Destroy(NowBox);
    }
}
