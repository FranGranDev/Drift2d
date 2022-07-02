using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Car : MonoBehaviour
{
    [Range(0, 3)]
    public float MaxSpeed;
    [Range(0, 1)]
    public float BackSpeed;
    [Range(0, 2f)]
    public float Acceleration;
    [Range(0, 2f)]
    public float TurnSpeed;
    [Range(0, 2f)]
    public float TurnRadius;
    [Range(0.1f, 1f)]
    public float Scrolling;
    [Range(0f, 3f)]
    public float Slide;
    [Range(0, 2f)]
    public float OffRoad;
    [Range(0, 1f)]
    public float BrakeSpeed;
    [Range(90f, 360f)]
    public float MaxWheelTurn;
    [Range(0.1f, 20f)]
    public float Weight;
    [Range(0f, 1000f)]
    public float Hp;
    private Coroutine GearChange;
    private Coroutine ReverseDrift;
    public bool GearChanging;
    public float GeatChangeTime;
    public int GearCount;
    public AnimationCurve DriftCurve;
    public AnimationCurve RadiusCurve;
    public AnimationCurve AccelerationCurve;
    [HideInInspector]
    public float MaxHp;

    private float StandartMaxSpeed;
    private float StandartAcceleration;
    private float StandartSlide;
    private float StandartHp;
    private float StandartTurn;
    public bool Ai;
    private bool HpTurn;
    public bool SlowMotion;
    public bool LightOn;
    [HideInInspector]
    public int WheelNum;
    [HideInInspector]
    public bool Destroyed;
    [HideInInspector]
    public enum GroundType { Track, TrackDirt, Ground, Sand, Puddle };
    public float Power;
    public int Gear;
    private Vector2 SetVelocity(float Power, float k, float t)
    {
        Vector2 Dir = new Vector2(Wheel[0].transform.up.x, Wheel[0].transform.up.y);
        return ((Vector2)transform.up + Rig.velocity.normalized * k + Dir * t).normalized * Power * 75f;
        //return transform.up * Power * 100f;
    }
    public float GetVelocity()
    {
        return Rig.velocity.magnitude > 0.1f ? Rig.velocity.magnitude / 75f : 0;
    }
    public float GearStop;
    public float Gaz;
    private float Brake;
    public float Turning;
    public float GetRelativeSpeed()
    {
        return Power / MaxSpeed;
    }
    public float Drift()
    {
        if (Destroyed)
            return 0;
        float Drift = (1f - Mathf.Abs(Vector2.Dot(Rig.velocity.normalized, transform.up.normalized))) * Clutch();
        if(Rig.velocity.magnitude < 0.1f)
        {
            return 0f;
        }
        else if(Drift < 0.0025f / Slide)
        {
            return Drift;
        }
        else if (Drift < 1f / Slide)
        {
            return Mathf.Sqrt(Drift) * 1.1f;
        }
        else
        {
            return Mathf.Sqrt(Drift) * 0.9f;
        }
    }
    public float SpecDrift()
    {
        return DriftCurve.Evaluate(Drift());
    }
    public float CamDrift()
    {
        float Drift = Mathf.Abs(1f - Vector2.Dot(Rig.velocity.normalized, transform.up.normalized)) * Clutch();
        if (Rig.velocity.magnitude < 0.1f)
        {
            return 0f;
        }
        else if (Drift < 1f)
        {
            return Mathf.Sqrt(Drift);
        }
        else
        {
            return Mathf.Sqrt(Drift) * 0.75f;
        }
    }
    public float DriftEffect()
    {
        if (isBack)
            return 0;
        return Drift() * Drift();
    }
    public float TurnDrift()
    {
        float DotX = Vector2.Dot(Rig.velocity.normalized, transform.right.normalized);
        return DotX >= 0 ? Mathf.Sqrt(DotX) : -Mathf.Sqrt(-DotX);
    }
    public float DriftingTurn(float k)
    {
        return Mathf.Abs(WheelRight() * Drift() * Drift() * Mathf.Sqrt(Slide) * (Gaz + 0.25f) * k / (GetVelocity() * 2f + 2f));
    }
    public float Radius()
    {
        return (RadiusCurve.Evaluate(Power * Mathf.Pow(TurnRadius, 0.25f)) + Acceleration * Gaz / (Power * 20f + 1));
    }
    public float FixedAcceleration()
    {
        return AccelerationCurve.Evaluate(Power / MaxSpeed);
    }
    public float FixTurn()
    {
        if(Mathf.Abs(Rig.angularVelocity) < 15f)
        {
            return 0.25f + Mathf.Abs(Rig.angularVelocity) * 0.1f;
        }
        else
        {
            return 1f;
        }
    }
    private float Perecladka(float TurnTo)
    {
        return 1;
        //return Mathf.Abs(Rig.angularVelocity + TurnTo) / (Mathf.Abs(Rig.angularVelocity) + 1) * 0.25f + 1;
    }
    public float FixSqrt(float a)
    {
        return a > 0 ? Mathf.Sqrt(a * 0.75f) : -Mathf.Sqrt(Mathf.Abs(a * 0.75f));    
    }
    public float WheelRight()
    {
        return Mathf.Sqrt(Mathf.Abs(Vector2.Dot(Rig.velocity.normalized, Wheel[0].transform.up) + 1f));
    }
    public float GetScrolling()
    {
        float Move = Gaz + Brake * 0.1f;
        float Scroll = (Rig.velocity - (isBack ? -1 : 1) * SetVelocity(Power, 0.1f, 0.1f)).magnitude * Mathf.Sqrt(Scrolling * Acceleration) * Move * Move / (SlowTurn * 5 + 1) / (Rig.velocity.magnitude / Acceleration * Power * 2f + 0.01f);
        return (Scroll <= 1 ? Scroll : 1);
        //return Mathf.Sqrt((Velocity(Power) - Rig.velocity).magnitude * Acceleration) / (Rig.velocity.magnitude * Acceleration * 2f + 0.25f) * (Drift() * 0.5f + 1f)
    }
    private float GetBraking()
    {
        return (Rig.velocity - SetVelocity(Power, 0.05f, 0.1f)).magnitude * Power / MaxSpeed * Brake;
    }
    private float SlowTurn;
    private float WheelTurn;
    private float RoadClutch;
    private float DriftClutch()
    {
        return (Road == GroundType.Puddle || Road == GroundType.Track) ? Mathf.Sqrt(1 / RoadClutch) : RoadClutch + (OffRoad - 1f) * (1f - RoadClutch) * 0.5f;
    }
    private float Clutch()
    {
        return clutch > 1 ? 1 : clutch < 0 ? 0 : clutch;
    }
    
    private int GetGear()
    {
        return 1 + Mathf.RoundToInt(Power / (MaxSpeed * 0.6f) / (Drift() * 0.25f + 1) * GearCount);
    }
    private float GetGearF()
    {
        return 1 + (Power / (MaxSpeed * 0.6f) * GearCount);
    }
    private float clutch;

    public bool isBack;
    public bool isReverseDrift;
    private float FixedBackSpeed()
    {
        return Power > BackSpeed * MaxSpeed ? 0 : 1;
    }

    private bool[] isPuddle;
    private bool[] isSand;
    private bool[] isDirt;
    private bool[] isDirtTrail; 
    [HideInInspector]
    public bool[] isDrift;
    private bool[] isBraking;
    private bool[] isSmoke;
    [HideInInspector]
    public bool isDriftSound;
    private bool isWindEffect;
    private bool isExhaust;
    public float EngineVolume;
    public float EnginePitch;
    public float SpeedTurn(float Turn)
    {
        return Mathf.Abs(Turn) > 0.1 ? GameData.TurnSensivity * 0.1f : 0.1f;
    }
    private float Const()
    {
        return Time.fixedDeltaTime * 100f;
    }

    private const float MaxTurnSpeed = 300f;

    public Rigidbody2D Rig;
    public Tower tower;
    private Material material;

    private GameObject PrevCrush;

    public Transform[] Wheel;
    public GroundType[] WheelSurface;
    public GroundType Road;
    public bool OffTrack()
    {
        int num = 0;
        for (int i = 0; i < WheelSurface.Length; i++)
        {
            if (WheelSurface[i] == GroundType.Ground || WheelSurface[i] == GroundType.Sand)
                num++;
        }
        return (num >= 4);
    }
    public Image HpSlider;
    public GameObject Headlights;
    public Transform Hit;
    public Transform[] Particle;

    private GameObject[] Wind;
    private GameObject[] Trail;
    private GameObject[] Sand;
    private GameObject[] Dirt;
    private GameObject[] Puddle;
    private GameObject[] Smoke;
    private GameObject[] Braking;

    public GameObject SandPref;
    public GameObject DirtPref;
    public GameObject PuddlePref;
    public GameObject TrailPref;
    public GameObject TrailDirtPref;
    public GameObject WindPref;
    public GameObject SmokePref;
    public GameObject Explosion;
    public AudioSource Engine;
    public AudioSource Effect;
    public Level level;


    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
        material.SetFloat("_Damage", 0);
        if(tower != null)
        {
            tower.GetComponent<SpriteRenderer>().material.SetFloat("_Damage", 0f);
        }
        
        if (LightOn)
        {
            Headlights.SetActive(true);
        }
        WheelNum = Wheel.Length;
        WheelSurface = new GroundType[WheelNum];
        isDrift = new bool[WheelNum];
        isSand = new bool[WheelNum];
        isPuddle = new bool[WheelNum];
        isSmoke = new bool[WheelNum];
        isBraking = new bool[WheelNum];
        isDirt = new bool[WheelNum];
        isDirtTrail = new bool[WheelNum];
        Wind = new GameObject[WheelNum];
        Sand = new GameObject[WheelNum];
        Puddle = new GameObject[WheelNum];
        Trail = new GameObject[WheelNum];
        Smoke = new GameObject[WheelNum];
        Braking = new GameObject[WheelNum];
        Dirt = new GameObject[WheelNum];
        Rig.mass = Weight;
        RoadClutch = 1f;
        StandartSlide = Slide;
        StandartMaxSpeed = MaxSpeed;
        StandartAcceleration = Acceleration;
        StandartHp = MaxHp;
        StandartTurn = TurnSpeed;
    }
    private void Start()
    {
        MaxHp = Hp;

    }

    public void Drive(float Drive, float Turn)
    {
        if (GameData.isReverting)
        {
            Rig.angularVelocity = 0f;
            Rig.velocity = Vector2.zero;
            return;
        }
        if (Destroyed)
        {
            Power = Mathf.Lerp(Power, 0, 0.25f);
            Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, 0, 0.1f);
            Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, 0.1f);
            Effects();
            return;
        }
        Turning = Turn;
        Gaz = Mathf.Lerp(Gaz, Drive >= 0 ? Drive : 0, 0.1f);
        Brake = Drive < 0 ? -Drive : 0;
        clutch = Mathf.Lerp(clutch, Road == GroundType.Track ? RoadClutch : RoadClutch + (OffRoad - 1f) * (1f - RoadClutch) * 0.5f, 0.15f * RoadClutch);
        WheelTurn = Mathf.Lerp(WheelTurn, Turning * 60f, TurnSpeed * TurnRadius * 0.1f);
        Rig.centerOfMass = Vector2.Lerp(Rig.centerOfMass, new Vector2(0, 0.5f * Drift()), 0.05f);

        if (Drive >= 0f)
        {
            if (!isBack)
            {
                float TurnTo = -(Turning) * Radius() * FixTurn() * Mathf.Pow(Clutch(), 0.75f) * (GetScrolling() * 1f + 1f) / (Drift() * Drift() * WheelRight() * 1f + 1) / (GetVelocity() / MaxSpeed / (Drift() + 1f) * 0.9f + 1f) * 390f;
                float DriftTurnTo = TurnDrift() * DriftClutch() * (Gaz * 0.8f + 0.2f) * Mathf.Pow(Acceleration, 0.75f) * Mathf.Sqrt(Slide) / Mathf.Sqrt(RoadClutch) / (GetScrolling() + 1) / ((Drift() * Drift() * 3.5f) * (GetVelocity() + 1f) * 1.5f + 1) * 1000f;
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, TurnTo * Perecladka(TurnTo), 0.045f / (SlowTurn * 1 + 1));
                float DriftRot = Mathf.Abs(Vector2.Dot(Rig.velocity.normalized, transform.up.normalized));
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, DriftTurnTo, DriftingTurn(1f) * 0.3f / (SlowTurn * 1 + 1));
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, 0, (1 - Drift()) / (Drift() + 1) / (Gaz * 3 + 1) / Mathf.Pow(TurnSpeed, 0.4f) * 0.02f);
                Power = Mathf.Lerp(Power, MaxSpeed * Drive * Mathf.Sqrt(Clutch()), FixedAcceleration() * Mathf.Sqrt((Clutch() + RoadClutch) * 0.5f) * (1 - GearStop) * Drive * (((Mathf.Abs(Gear) - Mathf.Abs(GetGearF())) + 2) / 2) * (GetScrolling() + 1) * (1 - Turn * 0.75f) / (GetVelocity() * 0.5f + 1f) / (Drift() * GetVelocity() * 1f + 1f) * (Acceleration / MaxSpeed) / (Drift() * 2f + 1f) / 200f);
                Power = Mathf.Lerp(Power, 0, (Drift() + Drift() * Drift()) * (RoadClutch) / (Slide + 0.5f) / (WheelRight() * 3 + 1) / (SlowTurn * 3f + 1) / (Gaz + 1) / (Acceleration + 0.6f) / (GetVelocity() * 10 + 1f) * (GetScrolling() * 5f + 1f) * 0.015f + Clutch() * (Turn * Turn * 0.0005f + 0.001f));
                float Zanos = Drift() * (Drift() * GetVelocity() * 3f + 1f) * (GetVelocity() * 1f + 1) * Clutch() * Mathf.Sqrt(Acceleration) * (2.5f - Gaz);
                Rig.velocity = Vector2.Lerp(Rig.velocity, SetVelocity(Power, Zanos, 0.1f * Clutch()), Clutch() / (Drift() * (Drift() * (1f - RoadClutch) + Mathf.Pow(Drift(), 3) * 0.7f + 1f) * Slide * (1f + Gaz) * 5.25f + 1f) / (GetVelocity() * 1f + 1f) / (SlowTurn + 1) * 0.2f);
                Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, Clutch() * Drift() * Drift() / Mathf.Sqrt(Slide) / (GetVelocity() * 1f + 1f) / (SlowTurn * 3f + 1) * 0.015f);

            }
            else
            {
                if((Vector2.Dot(Rig.velocity.normalized, transform.up) > -0.75f))
                {
                    GearChanging = false;
                    isBack = false;
                    After360();
                }
                if (GetVelocity() < 0.01f)
                {
                    GearChanging = false;
                    isBack = false;
                    Power = 0;
                }
                float TurnTo = Turning * Radius() * Clutch() * (GetVelocity() * GetVelocity() * 20 + 1) * 330f;
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, TurnTo, 0.05f);
                float ToPower = Brake * Clutch() * Acceleration * 0.05f;
                Power = Mathf.Lerp(Power, 0, Gaz * (Drift() / Slide * 0.3f + 1f) / (DriftingTurn(1f) + 1f) / (GetVelocity() * 4f + 1f) * 0.025f + Mathf.Abs(Turning) * 0.01f);
                Power = Mathf.Lerp(Power, 0, (Drift() * Drift()) * Clutch() / (GetVelocity() * 10 + 1f) * 0.05f + 0.001f);
                float Zanos = Drift() * (Drift() * GetVelocity() * 10f + 1) * Clutch() * Mathf.Sqrt(Acceleration);
                Rig.velocity = Vector2.Lerp(Rig.velocity, -SetVelocity(Power, Zanos, 0.1f * Clutch()), (Clutch() + RoadClutch) / (Drift() * (Drift() * GetVelocity() * 25 + 1f) * Slide * 5f + 1f) * 0.05f);
                Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, Clutch() * Drift() * Drift() / Mathf.Sqrt(Slide) / (GetVelocity() * 10f + 1f) * 0.005f);
            }
            StopReverse();
        }
        else if(Drive < 0f)
        {
            if ((Vector2.Dot(Rig.velocity.normalized, transform.up) < -0.1f) && Drive < -0.1f)
            {
                isBack = true;
            }

            if (Rig.velocity.magnitude > 1f && (Vector2.Dot(Rig.velocity.normalized, transform.up) > -0.1f))
            {
                GearChanging = false;
                isBack = false;
                float TurnTo = -FixSqrt(Turning) * SpecDrift() * Clutch() * Radius() * ((GetVelocity()) * Drift() * 5f + 1f) * Mathf.Sqrt(Slide) * 340f;
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, TurnTo, 0.2f / (Drift() * Mathf.Sqrt(Slide) * 4f + 1f) / (SlowTurn + 1));
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, 0, 0.005f / (SlowTurn + 1));
                Power = Mathf.Lerp(Power, 0, (Brake + 0.1f) / (Drift() * Slide * 0.5f + Turn * Turn + 1f) / (DriftingTurn(1f) + 1f) / (GetVelocity() * 4f + 1f) * 0.03f + Mathf.Abs(Turning) * 0.005f);
                float Zanos = Drift() * (Drift() * GetVelocity() * 15f + 1) * Clutch() * Mathf.Sqrt(Acceleration);
                Rig.velocity = Vector2.Lerp(Rig.velocity, SetVelocity(Power, Zanos, 0.1f), Brake / (Drift() * Mathf.Sqrt(Slide) * 2.5f + 1f) / (GetVelocity() * 5f + 1f) * Clutch() * 0.125f);
                Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, Drift() / (SlowTurn + 1) / Mathf.Sqrt(Slide) * 0.075f / (DriftingTurn(1f) + 1f) / (Rig.velocity.magnitude * 0.5f + 1f) * Clutch());
                StartReverse();
            }
            else
            {
                isBack = true;
                float TurnTo = Turning * SpecDrift() * Radius() * Clutch() * (GetVelocity() * GetVelocity() * 25 + 1) * 380f;
                Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, TurnTo, 0.04f);
                float ToPower = Brake * Clutch() * Acceleration * 0.05f;
                Power = Mathf.Lerp(Power, BackSpeed * Brake * MaxSpeed, FixedBackSpeed() * Acceleration * Mathf.Sqrt(Clutch()) * (1 - Turn * 0.75f) / (GetScrolling() * 5f + 1f)  / (Drift() * 5f + 1f) / 200f);
                Power = Mathf.Lerp(Power, 0, (Drift() * Drift()) * Clutch() / (GetVelocity() * 10 + 1f) * 0.05f);
                float Zanos = Drift() * (Drift() * GetVelocity() * 10f + 1) * Clutch() * Mathf.Sqrt(Acceleration) * 1.25f;
                Rig.velocity = Vector2.Lerp(Rig.velocity, -SetVelocity(Power, Zanos, 0.1f * Clutch()), (Clutch() + RoadClutch) / (Drift() * (Drift() * GetVelocity() * 15 + 1f) * Slide * 5f + 1f) * 0.04f);
                Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, Clutch() * Drift() * Drift() / Mathf.Sqrt(Slide) / (GetVelocity() * 5f + 1f) * 0.005f);
            }
        }

        if(!Ai)
        {
            if (isBack)
            {
                if (Gear != -1)
                {
                    if (GearChange != null)
                        StopCoroutine(GearChange);
                    GearChange = StartCoroutine(GearBack(Power));
                }
                    
            }
            else if (Mathf.Abs(Power) < 0.01f)
            {
                if (Gear != 0)
                {
                    if (GearChange != null)
                        StopCoroutine(GearChange);
                    GearChange = StartCoroutine(GearN(Power));
                }
                    
            }
            else if (Gear > GetGear() && !GearChanging)
            {
                if (GearChange != null)
                    StopCoroutine(GearChange);
                GearChange = StartCoroutine(GearDown(Power));
            }
            else if (Gear < GetGear() && Gear < GearCount && !GearChanging)
            {
                if (GearChange != null)
                    StopCoroutine(GearChange);
                GearChange = StartCoroutine(GearUp(Power));
            }
        }

        EngineSound();
        Effects();
    }

    public void UpdateCar(float MaxSpeed, float Acceleration, float Slide, float Turn, float MaxHp)
    {
        this.MaxSpeed += MaxSpeed;
        this.Acceleration += Acceleration;
        this.Slide += Slide;
        if (Slide > 1.1f)
            Slide = 1.1f;
        this.MaxHp *= MaxHp;
        this.TurnSpeed += Turn;
        Hp = MaxHp;
    }
    public void StandartCar()
    {
        MaxSpeed = StandartMaxSpeed;
        Acceleration = StandartAcceleration;
        Slide = StandartSlide;
        MaxHp = StandartHp;
        Hp = MaxHp;
    }

    private IEnumerator GearN(float speed)
    {
        Gear = 0;
        PlaySound("GearDown", 2f, false);
        GearStop = 0.75f;
        yield return new WaitForSeconds(GeatChangeTime);
        GearStop = 0f;
        yield break;
    }
    private IEnumerator GearBack(float speed)
    {
        Gear = -1;
        PlaySound("GearDown", 2f, false);
        GearStop = 0.75f;
        yield return new WaitForSeconds(GeatChangeTime * 2f);
        GearStop = 0f;
        yield break;
    }
    private IEnumerator GearUp(float speed)
    {
        Gear++;
        PlaySound("GearDown", 2f, false);
        if (Gear == 1)
            yield break;
        GearChanging = true;
        GearStop = 0.75f;
        yield return new WaitForSeconds(GeatChangeTime);
        GearStop = 0f;
        GearChanging = false;
        yield break;
    }
    private IEnumerator GearDown(float speed)
    {
        Gear--;
        PlaySound("GearDown", 2f, false);
        GearChanging = true;
        GearStop = 0.75f;
        yield return new WaitForSeconds(GeatChangeTime * 0.5f);
        GearStop = 0f;
        GearChanging = false;
        yield break;
    }

    private void StartReverse()
    {
        if(ReverseDrift == null)
            ReverseDrift = StartCoroutine(WaitTurn0(Turning));
    }
    private void StopReverse()
    {
        if(ReverseDrift != null)
        {
            StopCoroutine(ReverseDrift);
            ReverseDrift = null;
        }
            
        isReverseDrift = false;
    }
    private IEnumerator WaitTurn0(float StartTurning)
    {
        isReverseDrift = true;
        while(Mathf.Abs(Turning - StartTurning) < 0.1f)
        {
            yield return new WaitForFixedUpdate();
        }
        isReverseDrift = false;
        ReverseDrift = null;
        yield break;
    }
    private void After360()
    {
        StartCoroutine(SlowTurnCour());
    }
    private IEnumerator SlowTurnCour()
    {
        SlowTurn = 4;
        yield return new WaitForSeconds(0.25f);
        SlowTurn = 0;
        yield break;
    }

    public void Effects()
    {
        for (int i = 0; i < WheelNum; i++)
        {
            switch (WheelSurface[i])
            {
                case GroundType.Ground:
                    EndDirtTrail(i);
                    EndDrift(i);
                    EndPuddle(i);
                    EndSand(i);
                    EndSmoke(i);
                    EndBraking(i);
                    EndDirt(i);
                    break;
                case GroundType.Track:
                    EndDirtTrail(i);

                    if (((i >= 2 && (Drift() * (GetVelocity() * 2.5f + 1f) / Mathf.Sqrt(Slide) >= 0.7f) || (i < 2 && Drift() * (2f - WheelRight()) / Mathf.Sqrt(Slide) >= 0.8f)) && !isDrift[i]))
                    {
                        isDrift[i] = true;
                        Trail[i] = Instantiate(TrailPref, Wheel[i].position, transform.rotation, transform);
                    }
                    else if (((i >= 2 && Drift() * (GetVelocity() * 2.5f + 1f) / Mathf.Sqrt(Slide) < 0.6f) || (i < 2 && Drift() * (2f - WheelRight()) / Mathf.Sqrt(Slide) < 0.7f) && isDrift[i]) && Trail[i] != null)
                    {
                        EndDrift(i);
                    }

                    if (!isSmoke[i] && i >= 2 && RoadClutch >= 0.75f && (GetScrolling() > 0.1f || Drift() > 0.5f || GetBraking() > 0.5f))
                    {
                        Vector3 Position = new Vector3(Wheel[i].position.x, Wheel[i].position.y, -1);
                        isSmoke[i] = true;
                        Smoke[i] = Instantiate(SmokePref, Position, transform.rotation, transform);
                    }
                    else if (isSmoke[i] && GetScrolling() < 0.025f && Drift() < 0.3f && GetBraking() < 0.4f)
                    {
                        EndSmoke(i);
                    }
                    if (isSmoke[i] && Smoke[i] != null)
                    {
                        Smoke[i].GetComponent<ParticleSystem>().emissionRate = 500 * (GetScrolling() + Drift() * 0.5f + Gaz * 0.25f);
                    }

                    if (!isBack && Brake > 0.1f && ((i >= 2 && GetBraking() >= 0.3f) || (i < 2 && GetBraking() >= 0.4f)) && !isBraking[i])
                    {
                        isBraking[i] = true;
                        Braking[i] = Instantiate(TrailPref, Wheel[i].position, transform.rotation, transform);
                        DriftSound(true, Mathf.Sqrt(GetBraking() * 0.5f));
                    }
                    else if ((GetBraking() < 0.3f || Brake < 0.1f) && isBraking[i] && Braking[i] != null)
                    {
                        EndBraking(i);
                    }
                    EndPuddle(i);
                    EndSand(i);
                    EndDirt(i);
                    break;
                case GroundType.Sand:
                    if (i >= 2 && !isSand[i] && Gaz > 0.9f)
                    {
                        isSand[i] = true;
                        Sand[i] = Instantiate(SandPref, Wheel[i].position, transform.rotation, Wheel[i]);
                    }
                    else
                    {
                        EndSand(i);
                    }
                    if (isSand[i])
                    {
                        Sand[i].GetComponent<ParticleSystem>().emissionRate = Mathf.Floor(Rig.velocity.magnitude) * GetScrolling() * 2f;
                        Sand[i].GetComponent<ParticleSystem>().startSize = Mathf.Sqrt(Mathf.Floor(Rig.velocity.magnitude)) * GetScrolling() / 3f;
                    }
                    EndDrift(i);
                    EndDirtTrail(i);
                    EndPuddle(i);
                    EndSmoke(i);
                    EndBraking(i);
                    EndDirt(i);
                    break;
                case GroundType.TrackDirt:
                    EndDrift(i);
                    if (!isBack && i >= 2 && !isDirtTrail[i])
                    {
                        isDirtTrail[i] = true;
                        Trail[i] = Instantiate(TrailDirtPref, Wheel[i].position, transform.rotation, transform);
                    }
                    if (i >= 2 && !isDirt[i] && Gaz > 0.9f)
                    {
                        isDirt[i] = true;
                        Dirt[i] = Instantiate(DirtPref, Wheel[i].position, transform.rotation, Wheel[i]);
                    }
                    else
                    {
                        EndDirt(i);
                    }
                    if (isDirt[i])
                    {
                        Dirt[i].GetComponent<ParticleSystem>().emissionRate = Mathf.Floor(Rig.velocity.magnitude) * (GetScrolling() + 0.25f);
                        Dirt[i].GetComponent<ParticleSystem>().startSize = Mathf.Sqrt(Mathf.Floor(Rig.velocity.magnitude)) * (GetScrolling() + 0.25f) / 5f;
                    }
                    EndPuddle(i);
                    EndSmoke(i);
                    EndBraking(i);
                    EndSand(i);
                    break;
                case GroundType.Puddle:
                    if (!isPuddle[i] && i >= 2)
                    {
                        isPuddle[i] = true;
                        Puddle[i] = Instantiate(PuddlePref, Wheel[i].position, transform.rotation, Wheel[i]);
                    }
                    if (isPuddle[i])
                    {
                        Puddle[i].GetComponent<ParticleSystem>().emissionRate = Mathf.Floor(Rig.velocity.magnitude) * 25f;
                        Puddle[i].GetComponent<ParticleSystem>().startSize = Mathf.Sqrt(Mathf.Floor(Rig.velocity.magnitude)) / 25f;
                    }
                    EndDirtTrail(i);
                    EndDrift(i);
                    EndSand(i);
                    EndSmoke(i);
                    EndBraking(i);
                    EndDirt(i);
                    break;
            }
        }
        //bool DrSound = false;
        for(int i = 0; i < WheelNum; i++)
        {
            if((isDrift[i] || isBraking[i]) && (Drift() > 0.25f || GetBraking() > 0.3f))
            {
                DriftSound(true, Drift() + GetBraking());
                break;
            }
            if (i == 3 && (Drift() < 0.2f || GetBraking() < 0.1f))
                DriftSound(false, 1f);
        }

        if (Power >= MaxSpeed * 0.3f && !isWindEffect)
        {
            isWindEffect = true;
            Wind = new GameObject[Particle.Length];
            for (int i = 0; i < Particle.Length; i++)
            {
                Wind[i] = Instantiate(WindPref, Particle[i].position, transform.rotation, Particle[i]);
            }
        }
        else if (Power < MaxSpeed * 0.3f && isWindEffect)
        {
            isWindEffect = false;
            for (int i = 0; i < Particle.Length; i++)
            {
                Wind[i].transform.parent = null;
                Destroy(Wind[i], 2f);
            }
        }


        Effect.volume = (Drift() * 2f + GetScrolling() + GetBraking() * 2f) * GameData.Volume;
        Wheel[0].transform.localRotation = Quaternion.Euler(0, 0, -WheelTurn);
        Wheel[1].transform.localRotation = Quaternion.Euler(0, 0, -WheelTurn);

    }
    public void EngineSound()
    {
        Engine.volume = Mathf.Lerp(Engine.volume, EngineVolume * (Gaz + 1f) / 2 * GameData.Volume, 0.1f);
        Engine.pitch = Mathf.Lerp(Engine.pitch, EnginePitch * (1 + Mathf.Abs(GetGearF() - Mathf.Abs(Gear)) * ((Gaz * 0.75f + 0.25f) * (Drift() * Drift() + 1) + Power + Brake * 0.5f) * 1.25f), 0.1f);
    }
    public void TurnEngine(bool On)
    {
        StartCoroutine(EngineTurn(On));
    }
    private IEnumerator EngineTurn(bool On)
    {
        if (On)
        {
            PlaySound("EngineStart", 1f, false);
            yield return new WaitForSeconds(0.1f);
            Engine.Play();
        }
        else
        {
            Engine.Stop();
        }
        yield break;
    }

    public void TurnHp(bool On)
    {
        HpSlider.transform.parent.gameObject.SetActive(On);
        HpTurn = On;
        if (On)
        {
            StartCoroutine(HpSet());
        }
    }
    private IEnumerator HpSet()
    {
        while (HpTurn)
        {
            HpSlider.transform.parent.transform.up = -level.Ui.transform.up;
            HpSlider.fillAmount = Mathf.Lerp(HpSlider.fillAmount, Hp / MaxHp, 0.05f);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
    public void GetHit(float Damage)
    {
        Hp -= Damage;
        if (!Ai)
        {
            level.Ui.SetHp(Hp / MaxHp);
        }
        if (Hp <= 0)
        {
            Hp = 0;
            Destroy();
        }
        GearChanging = false;
    }
    public void AddHp(float Add)
    {
        if (Hp < 0)
        {
            Hp = Add;
            material.SetFloat("_Damage", 0);
            if(tower != null)
                tower.GetComponent<SpriteRenderer>().material.SetFloat("_Damage", 0f);
        }
        else
        {
            Hp = Hp + Add < MaxHp ? Hp + Add : MaxHp;
        }
    }
    public void AddHpProcent(float Add)
    {
        Hp += MaxHp * Add;
        if (Hp > MaxHp)
            Hp = MaxHp;
    }

    public void LikeNewCar()
    {
        for (int i = 0; i < WheelNum; i++)
        {
            EndDirtTrail(i);
            EndPuddle(i);
            EndSand(i);
            EndSmoke(i);
            EndDrift(i);
            EndBraking(i);
        }
        if(isWindEffect)
        {
            isWindEffect = false;
            for (int i = 0; i < Particle.Length; i++)
            {
                Wind[i].transform.parent = null;
                Destroy(Wind[i], 0f);
            }
        }
        
        Hp = MaxHp;
        Power = 0f;
        Gear = 0;
        Rig.velocity = Vector2.zero;
        Rig.angularVelocity = 0f;
        if (Destroyed)
        {
            TurnEngine(true);
            material.SetFloat("_Damage", 0);
            if(tower != null)
                tower.GetComponent<SpriteRenderer>().material.SetFloat("_Damage", 0f);
            Destroyed = false;
            if (Ai && level.Race == Level.RaceType.Drunk)
            {
                TurnHp(true);
            }
        }
    }
    public void EndDrift(int i)
    {
        if (isDrift[i] && Trail[i] != null)
        {
            Trail[i].transform.parent = null;
            Destroy(Trail[i], Trail[i].GetComponent<TrailRenderer>().time);
        }
        isDrift[i] = false;
    }
    public void EndDirtTrail(int i)
    {
        if (isDirtTrail[i] && Trail[i] != null)
        {
            Trail[i].transform.parent = null;
            Destroy(Trail[i], Trail[i].GetComponent<TrailRenderer>().time);
        }
        isDirtTrail[i] = false;
    }
    private void EndSmoke(int i)
    {
        if (isSmoke[i] && Smoke[i] != null)
        {
            isSmoke[i] = false;
            Smoke[i].transform.parent = null;
            Destroy(Smoke[i], Smoke[i].GetComponent<ParticleSystem>().startLifetime);
        }
        
    }
    private void EndPuddle(int i)
    {
        if (isPuddle[i] && Puddle[i] != null)
        {
            Puddle[i].transform.parent = null;
            Destroy(Puddle[i], 0.25f);
        }
        isPuddle[i] = false;
    }
    private void EndSand(int i)
    {
        if (isSand[i] && Sand[i] != null)
        {
            Destroy(Sand[i], 0.5f);
        }
        isSand[i] = false;
    }
    private void EndBraking(int i)
    {
        if (isBraking[i] && Braking[i] != null)
        {
            Braking[i].transform.parent = null;
            Destroy(Braking[i], Braking[i].GetComponent<TrailRenderer>().time);
        }
        isBraking[i] = false;
        

    }
    private void EndDirt(int i)
    {
        if (isDirt[i] && Dirt[i] != null)
        {
            Destroy(Dirt[i], 0.5f);
        }
        isDirt[i] = false;


    }


    public void PlaySound(string name, float Vol, bool Rand)
    {
        if (level.Audio.GetSound(name) == null)
            return;
        AudioSource Source = gameObject.AddComponent<AudioSource>();
        Sound sound = level.Audio.GetSound(name);
        Source.volume = sound.Volume * Vol * (Rand ? Random.Range(0.9f, 1.1f) : 1) * GameData.Volume;
        Source.pitch = sound.Pitch * (Rand ? Random.Range(0.75f, 1.25f) : 1); ;
        Source.clip = sound.Audio;
        Source.spatialBlend = 1f;
        Source.Play();
        StartCoroutine(OnSoundEnd(Source));
    }
    public void DriftSound(bool On, float volume)
    {
        if (!Effect.enabled)
            return;
        if (On)
        {
            if (!isDriftSound)
            {
                isDriftSound = true;
                Sound sound = level.Audio.GetSound("Drift");
                Effect.clip = sound.Audio;
                Effect.volume = sound.Volume * volume * GameData.Volume;
                Effect.pitch = sound.Pitch;
                Effect.loop = sound.loop;
                Effect.Play();
            }
        }
        else if (isDriftSound)
        {
            isDriftSound = false;
            Effect.Stop();
        }
    }

    private IEnumerator OnSoundEnd(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        Destroy(source);
    }
    public void SetColor(Color color)
    {
        material.SetColor("_CarColor", color);
    }

    public void Destroy()
    {
        if (Destroyed)
            return;
        if (!Ai)
        {
            level.OnCrush();
            level.PlayerHitEffect(2);
        }
        else
        {
            level.OnAiDestroyed();
        }
        TurnEngine(false);
        Rig.AddTorque(Random.Range(-500, 500));
        for (int i = 2; i < 4; i++)
        {
            EndSand(i);
            EndPuddle(i);
            EndDrift(i);
        }
        Instantiate(Explosion, transform.position, transform.rotation, null);

        TurnHp(false);
        material.SetFloat("_Damage", 1.5f);
        if(tower != null)
            tower.GetComponent<SpriteRenderer>().material.SetFloat("_Damage", 1.5f);
        Effect.enabled = false;
        PlaySound("Crush", 1, true);
        DriftSound(false, 0f);
        Destroyed = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (level == null)
            return;
        if (collision.tag == "Barrier")
        {
            if (collision.GetComponent<TilemapCollider2D>() == null || !collision.GetComponent<TilemapCollider2D>().enabled)
            {
                float k = Mathf.Abs(Vector2.Dot(collision.transform.up, Rig.velocity.normalized)) * 0.8f + 0.2f;
                GetHit(Rig.velocity.magnitude * k * 5f);
                Power *= 1 - Mathf.Abs(Vector2.Dot(collision.transform.up, Rig.velocity.normalized) * 0.9f + 0.1f);
                PlaySound("Hit", (k * Rig.velocity.magnitude / 25f), true);
                Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * k * Rig.velocity.magnitude / 5f));
            }
            else
            {
                GetHit(Rig.velocity.magnitude / 6f);
                PlaySound("Hit", (Rig.velocity.magnitude / 10f), true);
                Power = GetVelocity() * 0.9f;
                Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * GetVelocity() * 5f));
            }
            GearChanging = false;


            if (!Ai)
            {
                float Power = Mathf.Sqrt(GetVelocity() / MaxSpeed);
                level.PlayerHitEffect(Power);
            }
        }
        else if (collision.tag == "Enemy")
        {
            Destroy();
        }
        else if (collision.tag == "Car")
        {
            if (PrevCrush == null && !Destroyed)
            {
                Collider2D[] hit = Physics2D.OverlapCircleAll(Hit.position, 0.3f);
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].transform.root != transform)
                    {
                        PrevCrush = collision.transform.root.gameObject;
                        float DamageK = Mathf.Sqrt(Weight / PrevCrush.GetComponent<Car>().Weight);
                        float Damage = (Rig.velocity - PrevCrush.GetComponent<Rigidbody2D>().velocity * 0.5f).sqrMagnitude * DamageK * 0.5f;
                        if (!PrevCrush.GetComponent<Car>().Destroyed && !Ai)
                        {
                            float Power = Mathf.Sqrt(GetVelocity() / MaxSpeed);
                            level.PlayerWinEffect(Power);
                        }
                        PrevCrush.GetComponent<Car>().GetHit(Damage);


                        if (Rig.velocity.magnitude > 10)
                        {
                            Vector2 Direction = (transform.position - PrevCrush.transform.position).normalized;
                            Vector2 velocity = DamageK * Rig.velocity;
                            PrevCrush.GetComponent<Rigidbody2D>().velocity = velocity;
                            PrevCrush.GetComponent<Rigidbody2D>().angularVelocity = Vector2.Dot(transform.right, Direction) * DamageK * Rig.velocity.magnitude * 100f;
                            Rig.velocity -= velocity / DamageK * 0.5f;
                        }
                        float PowerDown = DamageK * (1 - Mathf.Sqrt(Mathf.Abs(Vector2.Dot(collision.transform.up, Rig.velocity.normalized)) * 0.5f + 0.5f));
                        Power *= PowerDown < 1 ? PowerDown : 1;
                        PlaySound("Hit", 1f, true);
                        Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * Damage));
                        return;
                    }
                }
            }

        }

    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Track")
        {
            RoadClutch = Mathf.Lerp(RoadClutch, level.TrackSlide, 0.01f);
            Road = GroundType.Track;
        }
        else if (collision.tag == "TrackDirt")
        {
            RoadClutch = Mathf.Lerp(RoadClutch, level.TrackSlide * 0.75f, 0.01f);
            Road = GroundType.TrackDirt;
        }

        else if (collision.tag == "Ground")
        {
            RoadClutch = Mathf.Lerp(RoadClutch, 0.75f, 0.01f);
            Road = GroundType.Ground;
        }
        else if (collision.tag == "Sand")
        {
            RoadClutch = Mathf.Lerp(RoadClutch, 0.5f, 0.01f);
            Road = GroundType.Sand;
        }
        else if (collision.tag == "Puddle")
        {
            RoadClutch = 0.3f * MaxSpeed / (GetVelocity() * 5f + 1f);
            Road = GroundType.Puddle;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Track" || collision.tag == "TrackDirt")
        {
            if (OffTrack())
            {
                if (!Ai)
                {
                    level.ExitTrack();
                }
            }

        }
        else if (collision.tag == "Car" &&
        collision.transform.root.gameObject == PrevCrush)
        {
            PrevCrush = null;
        }
    }
}
