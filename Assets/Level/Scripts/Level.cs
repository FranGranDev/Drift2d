using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using GoogleMobileAds.Api;
using UnityEngine.Rendering.Universal;

public class Level : MonoBehaviour
{
    public static Level active;
    public enum RaceType {Time, Race, Survive, Drunk, Drift, Free, Delivery, BattleRoyale, Menu};
    public RaceType Race;
    public enum RaceCondition { Normal, Wet, Night, NightWet };
    public RaceCondition Condition;
    [Range(0, 1)]
    public float TrackSlide;
    //[HideInInspector]
    public bool[] Check;
    public bool AllCheck()
    {
        for(int i = 0; i < Check.Length; i++)
        {
            if (!Check[i])
                return false;
        }
        return true;
    }
    [HideInInspector]
    public int NowRevert;
    [HideInInspector]
    private int SurvivedLap;
    [HideInInspector]
    public float StartedTime;
    //[HideInInspector]
    public bool LapStarted;
    [HideInInspector]
    public float NowTime;
    [HideInInspector]
    public float LapTime()
    {
        return (NowTime - StartedTime);
    }
    public float DriftTime;
    private bool LastChanse;
    private bool GetReward;
    private bool GetBonus;
    private Coroutine LapRestartCour;
    private Coroutine DieRestartCour;

    private int Delivered;
    private int DeliveredChallange;
    private float DeliveredTime;
    private float DeliveredTimeChallange;
    private float PrevVolume;

    public int Wave;
    public int AiDestroyed;
    public CarWave[] CarsWave;
    public Enemy[] EnemyCars;

    public Car Player;
    public CarRevert[] Reverts = new CarRevert[200];
    public DataRevert[] DataReverts = new DataRevert[200];
    private Coroutine RevertingCoroutine;
    public float RevertTime()
    {
        return (RevertStep * (Player.Destroyed ? 10f : 1f));
    }
    [Range(0, 1f)]
    public float RevertStep;
    public bool Revert;
    public MainCamera Cam;
    public Controller controller;
    public Coroutine OffTrack;
    public Coroutine DriftCalc;
    public AudioSystem Audio;
    public bool StaticCar;
    public Controller InputSystem; 
    public Animator EnemyRide;
    public Transform StartPoint;
    public Transform ToTarget;
    public Transform[] Spawn;
    public Delivery[] Delivery;
    public Transform[] CameraShow;
    private bool CameraReady;
    private bool MapOn;
    public int GetFatherSpawn()
    {
        int max = 0;
        for(int i = 0; i < Spawn.Length; i++)
        {
            if ((Spawn[i].transform.position - Player.transform.position).magnitude > (Spawn[max].transform.position - Player.transform.position).magnitude)
                max = i;
        }
        return max;
    }
    public GameData Game;
    public MainUi Ui;
    public Weather weather;
    public Animator anim;
    public Volume PostProc;
    public ChromaticAberration Chromatic;
    public ColorAdjustments Adjustments;
    public float HitEffect;
    public float WinEffect;
    public bool TipOn;
    public bool BonusOn;

    public void Awake()
    {
        active = this;
        //MobileAds.Initialize(initStatus => { });
        Game.Load();
        if (!StaticCar)
        {
            MakeCar();
        }
        InputSystem.car = Player;
        InputSystem.car.TurnEngine(true);
        controller.enabled = false;
        PostProc.profile.TryGet(out Chromatic);
        PostProc.profile.TryGet(out Adjustments);
    }
    public void Start()
    {
        controller.SetInput(GameData.InputType);

        if (!GameData.GameStarted)
        {
            OnFirstRun();
            GameData.GameStarted = true;
            Game.Save();
        }
        if (Game.IncreaseLoadInRow())
        {
            ShowAds("video");
        }
        else
        {
            OfferdBonus();
        }
        Audio.PlayMusic("MusicLevel" + Random.Range(0, 3), 0);
        TurnMap(false);
        Game.Ads.LoadAds();
        ShowAds("banner");

        switch (Race)
        {
            case RaceType.Time:
                StartCoroutine(OnTimeRaceStart());
                
                break;
            case RaceType.Free:
                StartCoroutine(OnFreeStart());

                break;
            case RaceType.Survive:
                StartCoroutine(OnSurviveStart());
                break;
            case RaceType.Drunk:
                StartCoroutine(OnDrunkStart());
                break;
            case RaceType.Drift:
                StartCoroutine(OnDriftStart());
                break;
            case RaceType.Delivery:
                StartCoroutine(OnDeliveryStart());
                break;
            case RaceType.Menu:
                
                break;
            case RaceType.BattleRoyale:
                StartCoroutine(OnBattleRoyaleStart());

                break;
        }
        switch(Condition)
        {
            case RaceCondition.Normal:

                break;
            case RaceCondition.Wet:
                weather.StartRain();
                break;
            case RaceCondition.NightWet:
                weather.StartRain();
                break;
            case RaceCondition.Night:
                weather.StartRain();
                break;
        }
    }
    public void ShowAds(string Type)
    {
        if (GameData.NoAds)
            return;
        switch (Type)
        {
            case "video":
                Game.Ads.ShowVideo();
                break;
            case "rewardedChanse":
                Game.Ads.ShowRewarded();
                break;
            case "rewardedBonus":
                Game.Ads.ShowRewarded();
                break;
            case "banner":
                Game.Ads.CreateBanner(true);
                break;
        }
        
    }
    public void HideBanner()
    {

    }

    private void OnFirstRun()
    {
        TurnMap(false);
        TipOn = true;
        Ui.TurnTip(true);
    }
    public void CloseTips()
    {
        TipOn = false;
        Ui.TurnTip(false);
    }

    public void MakeCar()
    {
        if (Player != null)
            Destroy(Player.gameObject);
        Player = Instantiate(Game.Cars[GameData.CarNum].car, StartPoint.position, StartPoint.rotation, null);
        Player.SetColor(Game.CarColor());
        Player.level = this;
        Player.name = "Player";
        //InputSystem.car = Player;
    }
    public void ExitTrack()
    {
        switch (Race)
        {
            case RaceType.Race:
                break;
            case RaceType.Survive:
                if(OffTrack != null)
                {
                    StopCoroutine(OffTrack);
                }
                OffTrack = StartCoroutine(WaitDie());
                break;
            case RaceType.Time:
                InvalidLapTime();
                RestartLap();
                break;
            case RaceType.Drift:
                /*
                if(DriftCalc != null)
                {
                    StopCoroutine(DriftCalc);
                    DriftCalc = null;
                }
                */
                break;
        }
        
    }
    public void EnterTrack()
    {
        switch (Race)
        {
            case RaceType.Race:
                break;
            case RaceType.Survive:

                Player.Destroy();
                break;
            case RaceType.Time:
                InvalidLapTime();
                break;
        }

    }
    private float MoneyRatio()
    {
        return Game.GetMoneyRatio() * Game.GetCarMoneyRatio();
    }
    public void OverlapFinish()
    {
        switch (Race)
        {
            case RaceType.Race:
                break;
            case RaceType.Survive:
                if (LapStarted && AllCheck())
                {
                    int Money = Mathf.RoundToInt(400f * MoneyRatio());
                    SurvivedLap++;
                    if (SurvivedLap >= Game.GetLapToWin())
                    {
                        Money += Mathf.RoundToInt(1000f * MoneyRatio() * Game.GetLapToWin());
                        SurvivedLap = 0;
                        Game.UpdateLapToWin();
                        Revert = false;
                        StartCoroutine(CreateEnemy());
                        ClearReverts();
                        PlaySound("WaveDone", 1, false);
                        PlayerWinEffect(0.1f);

                        StartCoroutine(CarRevertSave(1f));
                        StartCoroutine(EnemyRevertSave(1f));
                        StartCoroutine(DataRevertSave(1f));
                    }
                    PlusMoney(Money);
                    ClearReverts();
                }
                else
                {
                    Ui.TurnLapTime(true);
                    LapStarted = true;
                }
                break;
            case RaceType.Time:
                if (LapStarted && AllCheck())
                {
                    int Money = Mathf.RoundToInt((LapTime() * 15f + (Game.GetRecord() == 0 ? LapTime() : Game.GetRecord()) / LapTime() * 500f) * MoneyRatio());
                    if (LapTime() < Game.GetRecord() || Game.GetRecord() == 0)
                    {
                        PlayerWinEffect(0.5f);
                        Game.UpdateRecord(LapTime());
                        Money += Mathf.RoundToInt(500f * Mathf.Sqrt(Game.GetNowRecord() + 1f) * MoneyRatio());
                        PlaySound("LevelDone", 1, false);
                        Ui.ShowLapTime(1, LapTime());
                    }
                    else
                    {
                        PlayerWinEffect(0.5f);
                        Ui.ShowLapTime(2, LapTime());
                    }
                    PlusMoney(Money);
                    StartedTime = NowTime;
                    ClearReverts();
                }
                else
                {
                    Ui.TurnLapTime(true);
                    LapStarted = true;
                    StartedTime = NowTime;
                }
                break;
            case RaceType.Drift:
                if (LapStarted && AllCheck())
                {
                    float Value = (DriftTime / LapTime() * 10);
                    if (Value > Game.GetDriftBest())
                    {
                        PlayerWinEffect(0.5f);
                        Game.UpdateDrift(Value);
                        PlaySound("WaveDone", 1, false);
                    }
                    else
                    {
                        PlayerWinEffect(0.25f);
                    }
                    int Money = Mathf.RoundToInt(200f * MoneyRatio() * Value);
                    PlusMoney(Money);
                    Ui.PrintForTime(LanguagesSystem.Language.MainUIText.DriftScore + " " + Value.ToString("0.0") + "/10", 1f);
                    DriftTime = 0f;
                    ClearReverts();
                }
                else
                {
                    DriftTime = 0;
                    if (DriftCalc == null)
                    {
                        DriftCalc = StartCoroutine(CalculateDrift());
                    }
                    LapStarted = true;
                }
                StartedTime = NowTime;
                break;
        }
        
        for (int i = 0; i < Check.Length; i++)
        {
            Check[i] = false;
        }
    }
    public void PlusMoney(int Money)
    {
        GameData.Money += Money;
        Ui.GetMoney(Money);
        Game.Save();
    }
    public void OverlapCheck(int i)
    {
        if (!LapStarted)
            return;
        if(i == 0)
        {
            Check[i] = true;
        }
        else if(Check[i - i])
        {
            Check[i] = true;
        }
    }
    public void InvalidLapTime()
    {
        if (!LapStarted)
            return;
        LapStarted = false;
        for(int i = 0; i < Check.Length; i++)
        {
            Check[i] = false;
        }
        PlayerHitEffect(0.25f);
        Ui.InvalidTime();
    }
    private void OfferdReverse()
    {
        Ui.anim.SetTrigger("Reverse");
    }
    private void ReverseEnd()
    {
        Ui.anim.SetTrigger("ReverseEnd");
    }
    private void RestartLap()
    {
        LastChanse = true;
        if(LapRestartCour == null)
        {
            LapRestartCour = StartCoroutine(RestartLapCour(3f));
        }
    }
    public void OnCrush()
    {
        LastChanse = true;
        Ui.CrushCar();
        if (DieRestartCour == null)
        {
            DieRestartCour = StartCoroutine(RestartAfterDie(3f));
        }
    }
    private IEnumerator RestartLapCour(float delay)
    {
        OfferdReverse();
        yield return new WaitForSeconds(delay);
        ReverseEnd();
        float prevSpeed = Cam.Speed;
        Cam.Speed = 1f;
        controller.enabled = false;
        Player.LikeNewCar();
        LapStarted = false;
        for (int i = 0; i < Check.Length; i++)
        {
            Check[i] = false;
        }
        Player.transform.position = StartPoint.position;
        Player.transform.rotation = StartPoint.rotation;
        yield return new WaitForSeconds(1f);
        Ui.anim.ResetTrigger("ReverseEnd");
        Cam.Speed = prevSpeed;
        controller.enabled = true;
        LapRestartCour = null;
        yield break;
    }
    private IEnumerator RestartAfterDie(float delay)
    {
        OfferdReverse();
        yield return new WaitForSeconds(delay);
        ReverseEnd();
        Game.Save();

        DieRestartCour = null;
        yield return new WaitForSeconds(1f);
        Restart();
        yield break;
    }

    public void AddHpProcent(float add)
    {
        Player.AddHpProcent(add);
        Ui.SetHp(Player.Hp / Player.MaxHp);
    }

    public void OnDeliveryEnter()
    {
        Delivered++;
        PlusMoney(Mathf.RoundToInt(1000 * Game.GetCarMoneyRatio()));
        if(Delivered >= DeliveredChallange)
        {
            StartCoroutine(OnDeliveryDone(2f));
        }
    }
    private IEnumerator OnDeliveryDone(float time)
    {
        PlusMoney(Mathf.RoundToInt(10000 * Game.GetCarMoneyRatio() + 1800000 / (NowTime - DeliveredTime)));
        Ui.PrintForTime("Delivered successful Time - " + (NowTime - DeliveredTime).ToString("00.00"), time * 1.25f);
        yield return new WaitForSeconds(time);
        Player.transform.position = StartPoint.position;
        Player.transform.rotation = StartPoint.rotation;
        Player.LikeNewCar();
        Player.Hp = Player.MaxHp;
        Delivered = 0;


        for(int i = 0; i < Delivery.Length; i++)
        {
            Delivery[i].Restart();
            Delivery[i].gameObject.SetActive(false);
        }
        StartCoroutine(MakeDeliveryBox());
        yield break;
    }
    private IEnumerator MakeDeliveryBox()
    {
        DeliveredChallange = Delivery.Length;
        Delivered = 0;
        DeliveredTime = NowTime;

        int DeliveryMinus = Random.Range(Mathf.FloorToInt(Delivery.Length / 6f), Mathf.FloorToInt(Delivery.Length / 3f));
        bool[] Box = new bool[Delivery.Length];
        for (int i = 0; i < Delivery.Length; i++)
        {
            Box[i] = true;
            if(Random.Range(0, Mathf.FloorToInt(Delivery.Length / DeliveryMinus)) == 0)
            {
                Box[i] = false;
                DeliveredChallange--;
            }
        }
        for (int i = 0; i < Delivery.Length; i++)
        {
            if(Box[i])
            {
                Delivery[i].gameObject.SetActive(true);
                Delivery[i].Trig.Active = true;
            }
        }
        yield break;
    }

    public void PlaySound(string name, float Vol, bool Rand)
    {
        if (Audio.GetSound(name) == null)
            return;
        AudioSource Source = gameObject.AddComponent<AudioSource>();
        Sound sound = Audio.GetSound(name);
        Source.volume = sound.Volume * Vol * (Rand ? Random.Range(0.75f, 1.25f) : 1) * GameData.Volume;
        Source.pitch = sound.Pitch * (Rand ? Random.Range(0.75f, 1.25f) : 1); ;
        Source.clip = sound.Audio;
        Source.Play();
        StartCoroutine(OnSoundEnd(Source));
    }
    private IEnumerator OnSoundEnd(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        Destroy(source);
    }

    public void OnAiDestroyed()
    {
        PlaySound("EnemyKilled", 0.5f, false);
        AiDestroyed++;
        int Money = 0;
        if (Race != RaceType.Drunk)
        {
            Money = Mathf.RoundToInt(500f * MoneyRatio());
            PlusMoney(Money);
            return;
        }
        Money = Mathf.RoundToInt(100f * Mathf.Sqrt(Wave + 1f) * MoneyRatio());
        PlusMoney(Money);
        if(AiDestroyed >= CarsWave[Wave].car.Length)
        {
            StartCoroutine(OnWaveDone());
        }
        
    }
    public IEnumerator StartWave(int num)
    {
        if(num < CarsWave.Length)
        {
            AiDestroyed = 0;
            int number = num + 1;
            Ui.PrintForTime(LanguagesSystem.Language.MainUIText.WaveNum[num], 3f);
            yield return new WaitForSeconds(2f);
            EnemyCars = new Enemy[CarsWave[num].car.Length];
            for(int i = 0; i < CarsWave[num].car.Length; i++)
            {
                int SpawnNum = Random.Range(0, Spawn.Length);
                Vector3 RandPos = new Vector3(Random.Range(-5, 5f), Random.Range(-5, 5f), 0);
                EnemyCars[i] = new Enemy();
                EnemyCars[i].CarEnemy = Instantiate(CarsWave[num].car[i], Spawn[SpawnNum].position + RandPos, Spawn[SpawnNum].rotation, null);
                EnemyCars[i].CarEnemy.level = this;
                EnemyCars[i].CarEnemy.Ai = true;
                EnemyCars[i].CarEnemy.TurnHp(true);
                EnemyCars[i].CarEnemy.Hp = CarsWave[num].Difficulty * 100f;
                EnemyCars[i].CarEnemy.SetColor(CarsWave[num].color[i]);
                EnemyCars[i].Revert = new CarRevert[Reverts.Length];
                AiController controller = EnemyCars[i].CarEnemy.gameObject.AddComponent<AiController>();
                if (EnemyCars[i].CarEnemy.tower == null)
                {
                    controller.RaceType = AiController.Type.Drunk;
                }
                else
                {
                    controller.RaceType = AiController.Type.Tank;
                    
                }
                controller.Difficulity = CarsWave[num].Difficulty;
                controller.Target = Player;
                controller.car = EnemyCars[i].CarEnemy;
                
            }
            StartCoroutine(CarRevertSave(1f));
            StartCoroutine(EnemyRevertSave(1f));
            Player.GearChanging = false;
        }
        else
        {
            OnDrunkWin();
        }
        yield break;
    }
    private IEnumerator OnWaveDone()
    {
        ClearReverts();
        Revert = false;
        int number = Wave + 1;
        if (number > Game.GetMaxWave())
        {
            Game.UpdateMaxWave(number);
        }
        int Money = Mathf.RoundToInt(100f * MoneyRatio() * (Wave + 1f));
        PlusMoney(Money);
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.Wave + " " + number + " " + LanguagesSystem.Language.MainUIText.Done, 3f);
        PlaySound("WaveDone", 1, false);
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < EnemyCars.Length; i++)
        {
            if (EnemyCars[i].CarEnemy != null)
            {
                Destroy(EnemyCars[i].CarEnemy.gameObject);
                EnemyCars[i].CarEnemy = null;
            }
        }
        Player.transform.position = StartPoint.position;
        Player.transform.rotation = StartPoint.rotation;
        Player.LikeNewCar();
        Player.Hp = Player.MaxHp;
        Ui.SetHp(1);
        Wave += 1;
       
        StartCoroutine(StartWave(Wave));
        yield break;
    }

    public Car GetRandomCar()
    {
        int i = Random.Range(0, 19);
        return Game.Cars[i].car;
    }

    public IEnumerator CreateEnemy()
    {
        int Last = 0;
        for(int i = 0; i < EnemyCars.Length; i++)
        {
            if(EnemyCars[i].CarEnemy.Destroyed)
            {
                Destroy(EnemyCars[i].CarEnemy);
                for(int a = i; a < EnemyCars.Length - 1; a++)
                {
                    EnemyCars[a] = EnemyCars[a + 1];
                }
            }
            if (!EnemyCars[i].CarEnemy.Destroyed)
            {
                Last = i;
            }
        }
        Car[] TempCar = new Car[Last];
        for (int i = 0; i < Last; i++)
        {
            TempCar[i] = EnemyCars[i].CarEnemy;
        }
        int num = Mathf.RoundToInt(Game.GetLapToWin() * 2);
        EnemyCars = new Enemy[num];
        for(int i = 0; i < Last; i++)
        {
            EnemyCars[i] = new Enemy();
            EnemyCars[i].CarEnemy = TempCar[i];
        }
        for (int i = Last; i < num; i++)
        {
            EnemyCars[i] = new Enemy();
            int SpawnI = i - Mathf.FloorToInt(i / Spawn.Length) * Spawn.Length;
            Car NowCar = Instantiate(Game.Cars[GameData.CarNum].car);
            NowCar.transform.position = new Vector3(Spawn[SpawnI].position.x, Spawn[SpawnI].position.y, 0f);
            Vector2 Direction = (Player.transform.position - NowCar.transform.position).normalized;
            NowCar.transform.up = Direction;
            NowCar.SetColor(new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 0));
            NowCar.level = this;
            NowCar.Ai = true;
            NowCar.TurnHp(true);
            AiController controller = NowCar.gameObject.AddComponent<AiController>();
            controller.Difficulity = 0.5f;
            controller.Target = Player;
            controller.car = NowCar;
            controller.RaceType = AiController.Type.Survive;
            EnemyCars[i].CarEnemy = NowCar;
            yield return new WaitForSeconds(0f);
        }
        yield break;
    }
    public IEnumerator WaitDie()
    {
        yield return new WaitForSeconds(1f);
        if (Player.OffTrack())
        {
            Player.Destroy();
        }
        yield break;
    }

    private IEnumerator CalculateDrift()
    {
        while(!Player.Destroyed)
        {
            if(Player.Drift() > 0.3f && Player.Rig.velocity.magnitude > 2f && !Player.OffTrack() && !Player.isBack)
            {
                if(Player.Drift() * (Player.GetVelocity() * 5f + 1f) > 0.5f)
                    DriftTime += Time.fixedDeltaTime * Player.Drift() * (Player.GetVelocity() * 5f + 1f) * 3f;
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        DriftTime = 0f;
        yield break;
    }
    private IEnumerator ChecingkForWow()
    {
        while (!Player.Destroyed)
        {
            if(Player.Rig.angularVelocity > 270)
            {
                Ui.WOW();
                if(LapStarted)
                    DriftTime += 5f;
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            
        }
        DriftTime = 0f;
        yield break;
    }

    private IEnumerator ShowTargets(float timeforone)
    {
        yield return new WaitForSeconds(0.5f);
        Cam.isFollowing = false;
        CameraReady = false;
        Camera NowCam = Cam.GetComponent<Camera>();
        for (int i = 0; i < CameraShow.Length; i++)
        {
            while(((Vector2)Cam.transform.position - (Vector2)CameraShow[i].transform.position).magnitude > 0.5f)
            {
                Vector3 Pos = new Vector3(CameraShow[i].position.x, CameraShow[i].position.y, Cam.Height);
                Cam.transform.position = Vector3.Lerp(Cam.transform.position, Pos, 0.05f);
                NowCam.orthographicSize = Mathf.Lerp(NowCam.orthographicSize, CameraShow[i].position.z, 0.025f);
                Cam.transform.up = Vector2.Lerp(Cam.transform.up, CameraShow[i].transform.up, 0.05f);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(timeforone);
        }
        while (((Vector2)Cam.transform.position - (Vector2)Player.transform.position).magnitude > 1f)
        {
            Vector3 Pos = new Vector3(Player.transform.position.x, Player.transform.position.y, Cam.Height);
            Cam.transform.position = Vector3.Lerp(Cam.transform.position, Pos, 0.025f);
            Cam.transform.up = Vector2.Lerp(Cam.transform.up, Player.transform.up, 0.05f);
            NowCam.orthographicSize = Mathf.Lerp(NowCam.orthographicSize, Cam.Size, 0.025f);
            yield return new WaitForFixedUpdate();
        }
        Cam.isFollowing = true;
        CameraReady = true;
        yield break;
    }


    public IEnumerator OnSurviveStart()
    {
        while (TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.Survive + " " + Game.GetLapToWin() + " " + (Game.GetLapToWin() > 1 ? LanguagesSystem.Language.MainUIText.Laps : LanguagesSystem.Language.MainUIText.Lap) + " " + LanguagesSystem.Language.MainUIText.LeaveTrack_Die, 2f);
        Player.Power = Player.MaxSpeed * 0.05f;
        yield return new WaitForSeconds(2f);
        controller.enabled = true;
        Ui.TurnLapTime(true);
        Ui.TurnHp(true);
        Ui.SetHp(Player.Hp / Player.MaxHp);
        StartCoroutine(CreateEnemy());
        StartCoroutine(CarRevertSave(1f));
        StartCoroutine(EnemyRevertSave(1f));
        StartCoroutine(DataRevertSave(1f));
        yield break;
    }
    public IEnumerator OnDeliveryStart()
    {
        while (TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        StartCoroutine(MakeDeliveryBox());
        StartCoroutine(ShowTargets(0.1f));
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.DeliveryStart, 2f);
        while (!CameraReady)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        controller.enabled = true;
        Ui.TurnLapTime(true);
        Ui.TurnHp(true);
        Ui.SetHp(Player.Hp / Player.MaxHp);
        DeliveredTime = NowTime;
        StartCoroutine(CarRevertSave(1f));
        StartCoroutine(EnemyRevertSave(1f));
        StartCoroutine(DataRevertSave(1f));
        
        yield break;
    }
    public IEnumerator OnBattleRoyaleStart()
    {
        while (TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        Ui.PrintForTime("Winner winner Chicken dinner", 1f);
        int RandPos = Random.Range(0, Spawn.Length);
        Vector3 PlayerPos = new Vector3(Spawn[RandPos].position.x, Spawn[RandPos].position.y, 0f);
        Player.transform.position = PlayerPos;
        Player.transform.up = ((Vector2)StartPoint.transform.position - (Vector2)Player.transform.position).normalized;
        yield return new WaitForSeconds(2f);
        EnemyCars = new Enemy[Spawn.Length];
        for(int i = 0; i < Spawn.Length; i++)
        {
            EnemyCars[i] = new Enemy();
            if (i != RandPos)
            {
                Vector3 Pos = new Vector3(Spawn[i].position.x, Spawn[i].position.y, 0f);
                EnemyCars[i].CarEnemy = Instantiate(GetRandomCar(), Pos, Spawn[i].rotation, null);
                EnemyCars[i].CarEnemy.transform.up = ((Vector2)StartPoint.position - (Vector2)EnemyCars[i].CarEnemy.transform.position).normalized;
                EnemyCars[i].CarEnemy.level = this;
                AiController controller = EnemyCars[i].CarEnemy.gameObject.AddComponent<AiController>();
                controller.RaceType = AiController.Type.Royale;
                controller.Difficulity = 1f;
                controller.car = EnemyCars[i].CarEnemy;
                controller.Target = Player;
                controller.enabled = true;
                EnemyCars[i].CarEnemy.Ai = true;
                EnemyCars[i].CarEnemy.TurnHp(true);
            }
        }

        controller.enabled = true;
        Ui.TurnLapTime(true);
        DeliveredTime = NowTime;
        StartCoroutine(CarRevertSave(1f));
        StartCoroutine(EnemyRevertSave(1f));
        StartCoroutine(DataRevertSave(1f));

        yield break;
    }
    public IEnumerator OnTimeRaceStart()
    {
        while(TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        Ui.BreakTime(Game.GetRecord());
        yield return new WaitForSeconds(1f);
        controller.enabled = true;
        Ui.TurnLapTime(true);
        StartCoroutine(CarRevertSave(1f));
        StartCoroutine(DataRevertSave(1f));
        yield break;
    }
    public IEnumerator OnDrunkStart()
    {
        while (TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.DrinkStart, 2f);
        Ui.TurnHp(true);
        controller.enabled = true;
        Ui.SetHp(Player.Hp / Player.MaxHp);
        StartCoroutine(StartWave(0));
        StartCoroutine(CarRevertSave(2f));
        StartCoroutine(DataRevertSave(2f));
        StartCoroutine(EnemyRevertSave(2f));
        yield break;
    }
    public IEnumerator OnDriftStart()
    {
        while (TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.DriftStart + Game.GetDriftBest().ToString("0.0"), 2f);
        controller.enabled = true;
        Ui.TurnLapTime(true);
        DriftTime = 0;
        StartCoroutine(ChecingkForWow());
        StartCoroutine(CarRevertSave(0f));
        StartCoroutine(DataRevertSave(0f));
        yield break;
    }
    public IEnumerator OnFreeStart()
    {
        while (TipOn || BonusOn)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.FreeStart, 1f);
        controller.enabled = true;
        StartCoroutine(CarRevertSave(0f));
        StartCoroutine(DataRevertSave(0f));
        yield break;
    }

    private IEnumerator CarRevertSave(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        Revert = true;
        while (Revert)
        {
            Reverts[0] = new CarRevert(Player);
            yield return new WaitForSeconds(RevertTime());
            StartCoroutine(CarRevertRepeat());
        }
        yield break;
    }
    private IEnumerator CarRevertRepeat()
    {
        for (int i = 1; i < Reverts.Length; i++)
        {
            yield return new WaitForSeconds(RevertTime());
            Reverts[i] = Reverts[i - 1];
        }
        yield break;
    }

    private IEnumerator EnemyRevertSave(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        Revert = true;
        while (Revert)
        {
            for (int i = 0; i < EnemyCars.Length; i++)
            {
                if (EnemyCars[i].CarEnemy != null)
                {
                    EnemyCars[i].Revert[0] = new CarRevert(EnemyCars[i].CarEnemy);
                }
            }
            yield return new WaitForSeconds(RevertTime());
            for (int i = 0; i < EnemyCars.Length; i++)
            {
                StartCoroutine(EnemyRevertRepeat(i));
            }
        }
        yield break;
    }
    private IEnumerator EnemyRevertRepeat(int num)
    {
        if (num >= EnemyCars.Length || EnemyCars[num] == null)
            yield break;
        for (int a = 1; Revert && a < EnemyCars[num].Revert.Length; a++)
        {
            yield return new WaitForSeconds(RevertTime());
            EnemyCars[num].Revert[a] = EnemyCars[num].Revert[a - 1];
        }
        yield break;
    }

    private IEnumerator DataRevertSave(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        Revert = true;
        while (Revert)
        {
            DataReverts[0] = new DataRevert(this);
            yield return new WaitForSeconds(RevertTime());
            StartCoroutine(DataRevertRepeat());
        }
        yield break;
    }
    private IEnumerator DataRevertRepeat()
    {
        for (int i = 1; i < DataReverts.Length; i++)
        {
            yield return new WaitForSeconds(RevertTime());
            DataReverts[i] = DataReverts[i - 1];
        }
        yield break;
    }

    public void Effects()
    {
        Chromatic.intensity.value = (Player.GetRelativeSpeed() * 0.5f + Player.DriftEffect() * 0.75f);
        Adjustments.saturation.value = HitEffect * -100;
    }
    public void PlayerHitEffect(float Power)
    {
        anim.Play("PlayerHit");
        anim.speed = 1 / (Power * 9f + 1);
    }
    public void PlayerWinEffect(float Power)
    {
        anim.Play("PlayerWin");
        anim.speed = 1 / (Power * 9f + 1);
    }

    public void SetTargetToBox()
    {
        float min = 0;
        int minNum = 0;
        int start = 1;
        for(int i = 0; i < Delivery.Length; i++)
        {
            if(Delivery[i].Trig.Active && Delivery[i].gameObject.activeSelf)
            {
                min = ((Vector2)Delivery[i].transform.position - (Vector2)Player.transform.position).magnitude;
                minNum = i;
                start = i;
            }   
        }
        for(int i = 0; i < Delivery.Length; i++)
        {
            float Lenght = ((Vector2)Delivery[i].transform.position - (Vector2)Player.transform.position).magnitude;
            if (Lenght < min && Delivery[i].Trig.Active && Delivery[i].gameObject.activeSelf)
            {
                min = Lenght;
                minNum = i;
            }
        }
        Vector2 dir = ((Vector2)Delivery[minNum].transform.position - (Vector2)Player.transform.position).normalized;
        Vector2 RotDir = ((Vector2)Delivery[minNum].transform.position - (Vector2)ToTarget.transform.position).normalized;
        ToTarget.transform.up = RotDir;
        float Height = Cam.Cam.orthographicSize;
        float Width = Height * Cam.Cam.aspect;
        float x = dir.x * Height;
        float y = dir.y * Width;
        if (min > new Vector2(Height, Width).magnitude)
        {
            ToTarget.gameObject.SetActive(true);
            if (Mathf.Abs(x) > Width * 0.9f)
            {
                x = x > 0 ? Width * 0.9f : -Width * 0.9f;
            }
            if (Mathf.Abs(y) > Height * 0.9f)
            {
                y = y > 0 ? Height * 0.9f : -Height * 0.9f;
            }
            ToTarget.transform.position = new Vector3(x, y, 5f) + transform.position;
        }
        else
        {
            ToTarget.gameObject.SetActive(false);
        }
    }

    public void OfferdBonus()
    {
        if (!TipOn && Random.Range(0, 2) == 0)
        {
            BonusOn = true;
            Ui.TurnBonus(true);
        }
    }
    public void TakeBonus(int i)
    {
        switch(i)
        {
            case 0:
                Player.UpdateCar(-0.1f, 0.1f, 0.3f, 0.1f, 1f);
                break;
            case 1:
                Player.UpdateCar(0.5f, 0.2f, -0.1f, 0.05f, 1f);
                break;
            case 2:
                Player.UpdateCar(0f, 0f, 0f, 0f, 2f);
                break;
        }
        PlaySound("Buy", 1f, false);
        Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 50));
        CloseBonus();
        ShowAds("rewardedBonus");
    }
    public void CloseBonus()
    {
        StartCoroutine(CloseBonusCour());
    }
    private IEnumerator CloseBonusCour()
    {
        Ui.TurnBonus(false);
        yield return new WaitForSeconds(0.75f);
        BonusOn = false;
        yield break;
    }
    public void UserGotBonus()
    {
        Debug.Log("got");
        GetBonus = true;
    }
    public void CheckForBonus()
    {
        StartCoroutine(CheckForBonusCour());
    }
    private IEnumerator CheckForBonusCour()
    {
        SpecialResume();
        yield return new WaitForFixedUpdate();
        if(!GetBonus)
        {
            Player.StandartCar();
            PlaySound("NoMoney", 0.75f, false);
            Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 100));
        }
        else
        {
            PlaySound("Buy", 0.75f, false);
            Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 50));
        }
        GetBonus = false;
        CloseBonus();
        yield break;
    }

    public void RevertButtonDown()
    {
        RevertingCoroutine = StartCoroutine(RevertingCar());
    }
    public void RevertButtonUp()
    {
        StopCoroutine(RevertingCoroutine);
    }
    private IEnumerator RevertingCar()
    {
        while(true)
        {
            RevertCar();
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    public void RevertCar()
    {
        if(GameData.isReverting)
        {
            if (NowRevert + 2 < Reverts.Length && !Reverts[NowRevert + 1].Null)
            {
                NowRevert++;
                SetCar(NowRevert);
                SetData(NowRevert);
                for (int i = 0; i < EnemyCars.Length; i++)
                {
                    SetEnemy(NowRevert, i);
                }
            }
        }
    }

    public void UserGotChanse()
    {
        GetReward = true;
    }
    public void CheckReward()
    {
        StartCoroutine(CheckRewardCour());   
    }
    private IEnumerator CheckRewardCour()
    {
        yield return new WaitForSecondsRealtime(0.01f);
        GameData.isReverting = GetReward;
        if (!GetReward)
        {
            TurnOffRevertCar();
        }
        GetReward = false;
    }

    public void RevertUiTurn(bool On)
    {
        if (GameData.OnPause || Ui.RevertUi.gameObject.activeSelf)
            return;
        Ui.RevertTurn(On);
        Ui.anim.ResetTrigger("ReverseEnd");
        if (On)
        {
            TurnOnRevertCar();
            if(GameData.NoAds)
                return;
            if (LastChanse)
            {
                ShowAds("video");
            }
            else if (Game.IncreaseReversInRow())
            {
                ShowAds("video");
            }
        }
    }
    public void TurnOnRevertCar()
    {
        Audio.StopMusic();
        AudioListener.pause = true;
        Time.timeScale = 0;
        Ui.SetRevertButton(false);
        GameData.isReverting = true;
    }
    public void TurnOffRevertCar()
    {
        if (GameData.isReverting)
        {
            for (int i = 0; i < Reverts.Length - NowRevert; i++)
            {
                Reverts[i] = Reverts[i + NowRevert];
            }
            for (int i = Reverts.Length - NowRevert; i < Reverts.Length; i++)
            {
                Reverts[i].Null = true;
            }

            for(int i = 0; i < EnemyCars.Length; i++)
            {
                if (EnemyCars[i] == null)
                    return;
                for(int a = 0; a < EnemyCars[i].Revert.Length - NowRevert; a++)
                {
                    EnemyCars[i].Revert[a] = EnemyCars[i].Revert[a + NowRevert];
                }
            }
        }
        Time.timeScale = 1f;
        Audio.ResumeMusic();
        AudioListener.pause = false;
        Ui.SetRevertButton(true);
        Ui.anim.Play("ReverseIdleEnd");
        StartCoroutine(OffRevertCar());
        GameData.isReverting = false;
        NowRevert = 0;
    }
    private IEnumerator OffRevertCar()
    {
        Time.timeScale = 0f;
        Ui.RevertTurn(false);
        Ui.TurnCoolDown(true);
        for (int i = 3; i >= 0; i--)
        {
            Ui.SetCoolDown(i);
            yield return new WaitForSecondsRealtime(0.33f);
        }
        Ui.TurnCoolDown(false);
        Time.timeScale = 1f;
        yield break;
    }

    public void SetCar(int num)
    {
        Vector3 Position = new Vector3(Reverts[num].Position.x, Reverts[num].Position.y, -0.1f);
        Player.Turning = Reverts[num].Turn;
        Player.transform.position = Position;
        Player.transform.up = Reverts[num].Direction;
        Player.Rig.velocity = Reverts[num].Velocity;
        Player.Rig.angularVelocity = Reverts[num].AngularSpeed;
        Player.Gear = Reverts[num].Gear;
        Player.Power = Reverts[num].Power;
        for (int i = 0; i < Player.isDrift.Length; i++)
        {
            Player.EndDrift(i);
        }
        if (Reverts[num].WaitDie != null && !Player.OffTrack())
        {
            StopCoroutine(Reverts[num].WaitDie);
            Reverts[num].WaitDie = null;
        }
        if (!Reverts[num].Destroyed && Player.Destroyed)
        {
            if(DieRestartCour != null)
                StopCoroutine(DieRestartCour);
            DieRestartCour = null;
            Player.LikeNewCar();
            LastChanse = false;
        }
        Player.GearStop = 0;
        Player.Hp = Reverts[num].Hp;
        Player.EngineSound();
        Ui.SetHp(Player.Hp / Player.MaxHp);
        Cam.SetCam();
        Cam.transform.up = Reverts[num].CamDirection;
        Cam.transform.position = (Vector3)Reverts[num].CamPosition + Vector3.forward * Cam.Height;
        Cam.Cam.orthographicSize = Reverts[num].CamSize;
    }
    public void SetData(int num)
    {
        NowTime = DataReverts[num].time;
        StartedTime = DataReverts[num].StartedTime;
        DriftTime = DataReverts[num].DriftTime;
        LapStarted = DataReverts[num].LapStarted;
        Check = DataReverts[num].Check;
        for(int i = 0; i < Delivery.Length; i++)
        {
            if(Delivery[i].gameObject.activeSelf && Delivery[i].Trig.Active != DataReverts[num].DeliveryActive[i])
            {
                Delivery[i].Trig.Active = true;
                DataReverts[num].DeliveryActive[i] = true;
                Delivery[i].Restart();
                Delivered--;
            }
        }
        switch(Race)
        {
            case RaceType.Time:
                Ui.SetLapTime(NowTime - StartedTime);
                Ui.TurnLapTime(true);
                break;
            case RaceType.Drift:
                Ui.SetDriftScore(DriftTime / LapTime() * 10, 10);
                break;
            case RaceType.Drunk:
                Ui.SetHp(Player.Hp / Player.MaxHp);
                break;
        }
        if(LapStarted && LapRestartCour != null)
        {
            StopCoroutine(LapRestartCour);
            LapRestartCour = null;
            LastChanse = false;
        }
        if(LapStarted && !Player.Destroyed)
        {
            Ui.anim.Play("Idle", 1);
            if(LapRestartCour != null)
            {
                StopCoroutine(LapRestartCour);
            }
            if(DieRestartCour != null)
            {
                StopCoroutine(DieRestartCour);
            }
        }
    }
    public void SetEnemy(int num, int CarNum)
    {
        if (Reverts[num].Null || num > EnemyCars[CarNum].Revert.Length - 1)
            return;
        Vector3 Position = new Vector3(EnemyCars[CarNum].Revert[num].Position.x, EnemyCars[CarNum].Revert[num].Position.y, -0.1f);
        EnemyCars[CarNum].CarEnemy.transform.position = Position;
        EnemyCars[CarNum].CarEnemy.transform.up = EnemyCars[CarNum].Revert[num].Direction;
        EnemyCars[CarNum].CarEnemy.Rig.velocity = EnemyCars[CarNum].Revert[num].Velocity;
        EnemyCars[CarNum].CarEnemy.Rig.angularVelocity = EnemyCars[CarNum].Revert[num].AngularSpeed;

        EnemyCars[CarNum].CarEnemy.Power = EnemyCars[CarNum].Revert[num].Power;
        for (int i = 0; i < EnemyCars[CarNum].CarEnemy.isDrift.Length; i++)
        {
            EnemyCars[CarNum].CarEnemy.EndDrift(i);
        }

        if (!EnemyCars[CarNum].Revert[num].Destroyed && EnemyCars[CarNum].CarEnemy.Destroyed)
        {
            EnemyCars[CarNum].CarEnemy.LikeNewCar();
            AiDestroyed--;
        }
        EnemyCars[CarNum].CarEnemy.Hp = EnemyCars[CarNum].Revert[num].Hp;
        
    }
    public void ClearReverts()
    {
        for (int i = 0; i < Reverts.Length; i++)
        {
            Reverts[i] = new CarRevert();
            Reverts[i].Null = true;
        }
    }

    public void OnDrunkWin()
    {
        GameData.Money += 50000;
        Ui.GetMoney(50000);
        Ui.PrintForTime(LanguagesSystem.Language.MainUIText.CarBattleWin, 5f);
    }

    public void GoMainMenu()
    {
        Game.Ads.DestroyBanner();
        GameData.OnPause = false;
        Time.timeScale = 1f;
        Audio.StopMusic();
        Audio.ResumeMusic();
        AudioListener.pause = false;
        SceneManager.LoadScene(0);
    }
    public void GoCarSelect()
    {

    }
    public void GoTrack(int num)
    {
        SceneManager.LoadScene(num);
    }
    public void TurnMap(bool On)
    {
        Spectator.active.gameObject.SetActive(MapOn = On);
    }
    public void Restart()
    {
        if (MapOn)
            return;
        Time.timeScale = 1;
        GameData.OnPause = false;
        Audio.ResumeMusic();
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Pause()
    {
        if (Ui.RevertUi.gameObject.activeSelf || MapOn)
            return;
        if (!GameData.OnPause)
        {
            GameData.OnPause = true;
            Ui.PauseUi.gameObject.SetActive(true);
            switch (GameData.InputType)
            {
                case GameData.InputTypes.Buttons:
                    break;
                case GameData.InputTypes.Slider:
                    Ui.Slider.gameObject.SetActive(false);
                    Ui.Wheel.gameObject.SetActive(false);
                    break;
                case GameData.InputTypes.Wheel:
                    Ui.Wheel.gameObject.SetActive(false);
                    Ui.Slider.gameObject.SetActive(false);
                    break;
            }
            Ui.Gas.gameObject.SetActive(false);
            Ui.Brake.gameObject.SetActive(false);
            Time.timeScale = 0f;
            Audio.StopMusic();
            AudioListener.pause = true;
        }
        else
        {
            GameData.OnPause = false;
            Ui.PauseUi.gameObject.SetActive(false);
            switch (GameData.InputType)
            {
                case GameData.InputTypes.Buttons:
                    break;
                case GameData.InputTypes.Slider:
                    Ui.Slider.gameObject.SetActive(true);
                    Ui.Wheel.gameObject.SetActive(false);
                    break;
                case GameData.InputTypes.Wheel:
                    Ui.Wheel.gameObject.SetActive(true);
                    Ui.Slider.gameObject.SetActive(false);
                    break;
            }
            Ui.Gas.gameObject.SetActive(true);
            Ui.Brake.gameObject.SetActive(true);
            StartCoroutine(OffRevertCar());
            Audio.ResumeMusic();
            AudioListener.pause = false;
        }
    }
    public void Resume()
    {
        GameData.OnPause = false;
        Ui.PauseUi.gameObject.SetActive(false);
        switch (GameData.InputType)
        {
            case GameData.InputTypes.Buttons:
                break;
            case GameData.InputTypes.Slider:
                Ui.Slider.gameObject.SetActive(true);
                Ui.Wheel.gameObject.SetActive(false);
                break;
            case GameData.InputTypes.Wheel:
                Ui.Wheel.gameObject.SetActive(true);
                Ui.Slider.gameObject.SetActive(false);
                break;
        }
        Ui.Gas.gameObject.SetActive(true);
        Ui.Brake.gameObject.SetActive(true);
        Audio.ResumeMusic();
        AudioListener.pause = false;
    }
    public void SpecialPause()
    {
        Time.timeScale = 0f;
        Audio.StopMusic();
        AudioListener.pause = true;
    }
    public void SpecialResume()
    {
        Ui.PauseUi.gameObject.SetActive(false);
        if (GameData.isReverting)
            return;
        GameData.OnPause = false;
        Time.timeScale = 1f;
        Audio.ResumeMusic();
        AudioListener.pause = false;
        switch (GameData.InputType)
        {
            case GameData.InputTypes.Buttons:
                break;
            case GameData.InputTypes.Slider:
                Ui.Slider.gameObject.SetActive(true);
                Ui.Wheel.gameObject.SetActive(false);
                break;
            case GameData.InputTypes.Wheel:
                Ui.Wheel.gameObject.SetActive(true);
                Ui.Slider.gameObject.SetActive(false);
                break;
        }
        Ui.Gas.gameObject.SetActive(true);
        Ui.Brake.gameObject.SetActive(true);
    }

    public void Exit()
    {
        if (MapOn)
            return;
        GoMainMenu();
    }

    public void Load()
    {
        Game.Load();
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause && !AdMob.ShowingAds)
        {
            Pause();
        }
    }
    private void OnApplicationQuit()
    {
        Game.Save();
    }

    public void FixedUpdate()
    {
        switch (Race)
        {
            case RaceType.Time:
                if (LapStarted)
                {
                    Ui.SetLapTime(LapTime());
                }
                break;
            case RaceType.Survive:
                Ui.SetSurvivedLap(SurvivedLap, Game.GetLapToWin());
                
                break;
            case RaceType.Drift:
                Ui.SetDriftScore(DriftTime / LapTime() * 10, 10);

                break;
            case RaceType.Free:
                
                break;
            case RaceType.Delivery:
                SetTargetToBox();
                Ui.SetDeliveryLap(Delivered, DeliveredChallange, (NowTime - DeliveredTime));
                break;
            case RaceType.BattleRoyale:
                Ui.SetBattleRoyaleScore(16 - AiDestroyed, 16);
                break;
        }
        NowTime += Time.fixedDeltaTime;
        Ui.SetGear(Player.Gear);
        Ui.SetSpeed(Player.Power);
        Effects();
        if (Spectator.active != null)
        {
            Spectator.active.FixedUpdate();
        }
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            Time.timeScale = 0;
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Time.timeScale = 1;
        }
        if(Input.GetKeyDown(KeyCode.V))
        {

        }
    }
}

[System.Serializable]
public class CarWave
{
    public string Text;
    public int num;
    public Color[] color;
    public Car[] car;
    [Range(0.1f, 1f)]
    public float Difficulty;
}
[System.Serializable]
public class CarRevert
{
    public Vector2 Position;
    public Vector2 Direction;

    public Vector2 CamPosition;
    public Vector2 CamDirection;
    public float CamSize;

    public Vector2 Velocity;
    public float AngularSpeed;

    public Coroutine WaitDie;

    public float Turn;
    public float Power;
    public float Hp;
    public int Gear;
    public bool Destroyed;

    public bool Null;

    public CarRevert(Car Player)
    {
        Turn = Player.Turning;
        Position = Player.transform.position;
        Direction = Player.transform.up;
        CamDirection = Player.level.Cam.transform.up;
        CamPosition = Player.level.Cam.transform.position;
        CamSize = Player.level.Cam.Cam.orthographicSize;
        Gear = Player.Gear;
        WaitDie = Player.level.OffTrack;

        Velocity = Player.Rig.velocity;
        AngularSpeed = Player.Rig.angularVelocity;

        Power = Player.Power;
        Hp = Player.Hp;
        Destroyed = Player.Destroyed;

        Null = false;
    }
    public CarRevert()
    {
        Turn = 0;
        Position = Vector2.zero;
        Direction = Vector2.up;
        CamDirection = Vector2.up;
        CamPosition = Position;
        CamSize = 10f;
        Gear = 0;
        WaitDie = null;

        Velocity = Vector2.zero;
        AngularSpeed = 0f;

        Power = 0f;
        Hp = 0f;
        Destroyed = false;

        Null = false;
    }
}
[System.Serializable]
public class DataRevert
{
    public int money;

    public float time;
    public float StartedTime;
    public bool LapStarted;
    public float DriftTime;
    public int AiKilled;
    public bool[] Check;
    public bool[] DeliveryEnable;
    public bool[] DeliveryActive;

    public bool Null;
    public DataRevert(Level level)
    {
        money = GameData.Money;
        time = level.NowTime;
        StartedTime = level.StartedTime;
        LapStarted = level.LapStarted;
        DriftTime = level.DriftTime;
        AiKilled = level.AiDestroyed;
        Check = new bool[level.Check.Length];
        DeliveryActive = new bool[level.Delivery.Length];
        DeliveryEnable = new bool[level.Delivery.Length];
        for(int i = 0; i < Check.Length; i++)
        {
            Check[i] = level.Check[i];
        }
        for(int i = 0; i < level.Delivery.Length; i++)
        {
            if(level.Delivery[i].gameObject.activeSelf)
            {
                DeliveryEnable[i] = true;
                if(level.Delivery[i].Trig.Active)
                {
                    DeliveryActive[i] = true;
                }
                else
                {
                    DeliveryActive[i] = false;
                }
            }
            else
            {
                DeliveryEnable[i] = false;
                DeliveryActive[i] = false;
            }
        }
        Null = false;
    }
    public DataRevert()
    {
        money = GameData.Money;
        time = 0;
        StartedTime = 0f;
        LapStarted = false;
        DriftTime = 0f;
        AiKilled = 0;
        DeliveryEnable = new bool[0];
        DeliveryEnable = new bool[0];
        Check = new bool[0];
        Null = true;
    }
}
[System.Serializable] 
public class Enemy
{
    public Car CarEnemy;
    public CarRevert[] Revert;

    public Enemy()
    {
        CarEnemy = null;
        Revert = new CarRevert[200];
    }
    public Enemy(Car car)
    {
        CarEnemy = car;
        Revert = new CarRevert[25];
    }
}