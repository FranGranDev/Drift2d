using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
{
    public enum Type {Static, Drunk, Race, Survive, Tank, Royale, StaticTank}
    public Type RaceType;
    [Range(0.1f , 1)]
    public float Difficulity;
    [HideInInspector]
    public Car car;

    private bool FireReady;
    private bool Reloading;

    public Car Target;
    public AiCheck[] Check;
    private Transform PrevCheck;
    public int NowCheck;
    private Vector2 CurrentTarget;
    private Vector2 GetDirection()
    {
        return (Check[NowCheck].transform.position - Check[NowCheck + 1 < Check.Length ? NowCheck + 1 : 0].transform.position).normalized;
    }
    private float Turn;
    private float Acceleration;
    private float Braking;
    private float Collision;
    private bool OffTrack;



    public void Follow()
    {
        CurrentTarget = Target.transform.position + Target.transform.up * Target.Rig.velocity.magnitude *  (1f - Mathf.Abs(Vector2.Dot(Target.transform.up, Target.transform.right)));

        Vector2 Direction = CurrentTarget - (Vector2)transform.position;
        float DotY = Vector2.Dot(transform.up, Direction.normalized);
        float DotX = Vector2.Dot(transform.right, Direction.normalized);
        DotX = DotX * Mathf.Abs(DotX);
        if (DotY > 0)
        {
            Turn = Mathf.Lerp(Turn, DotX, Difficulity * 0.1f);
        }
        else
        {
            Turn = Mathf.Lerp(Turn, DotX > 0 ? 1 : -1, Difficulity * 0.1f);
        }

        Acceleration = Mathf.Lerp(Acceleration, 1.5f - Turn * Turn * 0.1f, 0.25f);
        car.Drive(Acceleration, Turn);
    }
    public void Stay()
    {
        car.Drive(0, 0);
    }
    public void TankFollow()
    {
        Follow();
        TankFire();
    }

    public void TankFire()
    {
        if (!car.Destroyed)
        {
            CurrentTarget = (Target.transform.position + Target.transform.up * Target.Rig.velocity.magnitude / car.tower.BulletSpeed * 500) - transform.position;
            car.tower.transform.up = Vector2.Lerp(car.tower.transform.up, CurrentTarget.normalized, car.tower.TowerSpeed * 0.1f);
            if (FireReady && Vector2.Dot(car.tower.transform.up, CurrentTarget.normalized) > 0.99f)
            {
                car.tower.Fire();
                car.PlaySound("GunFire", 1f, true);
                FireReady = false;
            }
            if (!FireReady && !Reloading)
            {
                StartCoroutine(Reload());
            }
        }
    }
    private IEnumerator Reload()
    {
        Reloading = true;
        yield return new WaitForSeconds(car.tower.ReloadTime);
        FireReady = true;
        Reloading = false;
    }
    public void Race()
    {
        RaycastHit2D LeftHit = Physics2D.Raycast(transform.position, -transform.right, 2f, 1 << 8);
        RaycastHit2D RightHit = Physics2D.Raycast(transform.position, transform.right, 2f, 1 << 8);
        //RaycastHit2D ForwardHit = Physics2D.Raycast(transform.position, -transform.right, 5f);
        if(LeftHit.collider != null)
        {
            Collision = -10f / (LeftHit.distance) * (1 - Vector2.Dot(transform.up, LeftHit.collider.transform.up));
        }
        else if(RightHit.collider != null)
        {

            Collision = 10f / (RightHit.distance) * (1 - Vector2.Dot(transform.up, RightHit.collider.transform.up));
        }
        else
        {
            Collision = 0f;
        }

        CurrentTarget = Check[NowCheck].transform.position;
        float Distance = (transform.position - Check[NowCheck].transform.position).magnitude;
        

        Vector2 Direction = CurrentTarget - (Vector2)transform.position;
        float DotY = Vector2.Dot(transform.up, Direction.normalized);
        float DotX = Vector2.Dot(transform.right, Direction.normalized);
        float BrakeDotX = Vector2.Dot(transform.right, Direction + GetDirection() / 2);

        float AccelerationPower = 1;
        

        if (car.Road == Car.GroundType.Track)
        {
            if (DotY > 0)
            {
                Turn = Mathf.Lerp(Turn, BrakeDotX > 0 ? 1 : -1 * Mathf.Sqrt(Mathf.Abs(BrakeDotX)), Difficulity * 0.1f);
            }
            else
            {
                Turn = Mathf.Lerp(Turn, DotX > 0 ? 1 : -1, Difficulity * 0.1f);
            }
            AccelerationPower = (1 - Turn * 0.25f) * Check[NowCheck].Acceleration;
            Braking = Mathf.Abs(BrakeDotX) * Check[NowCheck].Braking * car.Power * 5f;
            /*
            if (Distance > Check[NowCheck].BrakeDistance)
            {
                AccelerationPower = (1 - Turn * 0.25f);
                Braking = Mathf.Abs(DotX) * Direction.magnitude * car.Power * 0.25f;
                
            }
            else 
            {
                AccelerationPower = 1 / (Distance + 1f) < 1 ? 1 / (Distance + 1f) : 1;
                Braking = Check[NowCheck].Braking;
            }
            */
        }
        else
        {
            if (car.Power > 0.1f)
            {
                AccelerationPower = 0f;
                Braking = 1f;
            }
            else
            {
                AccelerationPower = 1f;
                Braking = 0f;
            }
        }
        

        Acceleration = Mathf.Lerp(Acceleration, AccelerationPower * Check[NowCheck].Acceleration, 0.1f);
        Acceleration = Mathf.Lerp(Acceleration, -Braking, Braking * (!OffTrack ? Check[NowCheck].Braking : 1f));

        car.Drive(Acceleration, Turn);
    }

    public delegate void CarAction();
    public CarAction Action;
    public void Start()
    {
        car = GetComponent<Car>();
        //Target = car.level.Player;
        switch (RaceType)
        {
            case Type.Race:
                {
                    Action = Race;
                    break;
                }
            case Type.Royale:
                {
                    Action = Follow;
                    break;
                }
            case Type.Drunk:
                {
                    Action = Follow;
                    break;
                }
            case Type.Survive:
                {
                    Action = Follow;
                    break;
                }
            case Type.Tank:
                {
                    Action = TankFollow;
                    break;
                }
            case Type.Static:
                {
                    Action = Stay;
                    break;
                }
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (RaceType == Type.Race)
        {
            if (collision.tag == "Track" && car.OffTrack() && !OffTrack)
            {
                NowCheck++;
                if (NowCheck >= Check.Length - 1)
                    NowCheck = 0;
                OffTrack = true;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (RaceType == Type.Race)
        {
            if (collision.tag == "Track" && OffTrack)
            {
                OffTrack = false;
            }
            if (collision.GetComponent<AiCheck>() != null && collision.transform != PrevCheck)
            {
                NowCheck++;
                if (NowCheck >= Check.Length - 1)
                    NowCheck = 0;
                PrevCheck = collision.transform;
            }
        }
    }

    public void FixedUpdate()
    {
        Action();
    }
}
