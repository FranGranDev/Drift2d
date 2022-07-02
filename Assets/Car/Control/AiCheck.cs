using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AiCheck : MonoBehaviour
{
    [Range(0, 1f)]
    public float Acceleration;
    [Range(0, 1f)]
    public float Braking;
    public Vector2 RandonPoints;
    private BoxCollider2D Collider;
    private void Awake()
    {
        Collider = GetComponent<BoxCollider2D>();
        RandonPoints = (Vector2)transform.position + Collider.size;
    }
}
