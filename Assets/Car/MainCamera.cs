using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Level level;
    private Car Target;
    private Rigidbody2D Rig;
    public float Height;
    public float Offset;
    public float Size;
    public float MaxSize;
    public bool isFollowing;
    [Range(0, 10)]
    public float Speed;
    [Range(0.01f, 0.1f)]
    public float RotationSpeed;
    public Camera Cam;

    private void Awake()
    {
       
        Cam = GetComponent<Camera>();
    }
    private void Start()
    {
        
        Target = level.Player;
        Rig = Target.GetComponent<Rigidbody2D>();
    }
    private float PlusSize(float Velocity)
    {
        if ((Size + Velocity / 5f) < MaxSize)
            return Size + Velocity / 5f;
        else
            return MaxSize;
    }
    public void SetCam()
    {
        float Velocity = Rig.velocity.magnitude;
        transform.rotation = Target.transform.rotation;
        Cam.orthographicSize = PlusSize(Velocity);
        Vector2 PosOffset = transform.up * (Offset - Mathf.Sqrt(Velocity) * 0.5f);
        transform.position = (Vector2)Target.transform.position - PosOffset;
        transform.position = new Vector3(transform.position.x, transform.position.y, Height);
    }
    public void Follow()
    {
        float Velocity = Rig.velocity.magnitude;
        transform.rotation = Quaternion.Lerp(transform.rotation, Target.transform.rotation, RotationSpeed / (Target.CamDrift() * 3f + 0.5f));
        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, PlusSize(Velocity), 0.1f);
        Vector2 PosOffset = transform.up * (Offset - Mathf.Sqrt(Velocity) * 0.5f);
        transform.position = Vector2.Lerp(transform.position, (Vector2)Target.transform.position - PosOffset, Speed / 10f / (Mathf.Pow(Target.Drift(), 2) * 3 + 1));
        transform.position = new Vector3(transform.position.x, transform.position.y, Height);
    }

    private void FixedUpdate()
    {
        if(isFollowing)
        {
            Follow();
        }
    }
    private void Update()
    {
        transform.rotation = new Quaternion(0, 0, transform.rotation.z, transform.rotation.w);
    }
}
