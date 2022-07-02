using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing;

public class Menu : MonoBehaviour , IStoreListener
{
    private enum MenuNavightion {Idle, CarSelect, Track, Special, Store, Settings};
    private MenuNavightion Navightion;

    public Transform AnimCar;
    public GameData Game;
    public Slider Rslider;
    public Slider Gslider;
    public Slider Bslider;
    public GameObject SelectCanvas;
    public Image SelectPlate;
    public RectTransform FirstLoadSelect;

    public Color GetRareColor(int Enum)
    {
        switch(Enum)
        {
            case 0:
                {
                    return new Color(0.968f, 0.952f, 0.956f);
                }
            case 1:
                {
                    return new Color(0.654f, 0.678f, 0.945f);
                }
            case 2:
                {
                    return new Color(0.964f, 0.415f, 0.878f);
                }
            case 3:
                {
                    return new Color(0.949f, 0.862f, 0.450f);
                }
        }
        return Color.gray;
    }
    public string TimeLapString(float time)
    {
        string Minutes = Mathf.Floor(time / 59).ToString("00") + ",";
        string Seconds = (time - Mathf.Floor(time / 59) * 59).ToString("00.00");
        return (Minutes + Seconds);
    }

    public GameObject carSelect;
    public Slider Acceleration;
    public Slider MaxSpeed;
    public Slider Difficulty;
    public TextMeshProUGUI Cost;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI CarTerms;
    public GameObject LocalLock;

    public GameObject trackSelect;
    public GameObject TrackLock;
    public Slider TrackDifficulty;
    public Slider TrackMoneyRatio;
    public Slider SensivitySlider;
    public Slider VibrationSlider;

    public TextMeshProUGUI SelectTrackTakeText;
    public TextMeshProUGUI TrackDescription;
    public TextMeshProUGUI TrackTerms;
    public GameObject TrackLocalLock;

    public Image[] Flags;
    public Image[] InputImagine;

    public TextMeshProUGUI GoTrackText;
    public TextMeshProUGUI Track_Name;
    public TextMeshProUGUI Track_Difficulity;
    public TextMeshProUGUI Track_MoneyRatio;
    public TextMeshProUGUI Track_Racord;
    public TextMeshProUGUI Track_Play;
    public TextMeshProUGUI Track_Back;
    public TextMeshProUGUI Track_BackToMenu;
    public TextMeshProUGUI Track_Cost;
    public TextMeshProUGUI Track_CostNum;

    public TextMeshProUGUI GoSuperTrackText;
    public TextMeshProUGUI Super_Back;

    public TextMeshProUGUI GoSelectText;
    public TextMeshProUGUI Select_Name;
    public TextMeshProUGUI Select_BackToMain;
    public TextMeshProUGUI Select_Color;
    public TextMeshProUGUI Select_Acceleration;
    public TextMeshProUGUI Select_MaxSpeed;
    public TextMeshProUGUI Select_Drift;
    public TextMeshProUGUI Select_CarCost;
    public TextMeshProUGUI Select_Description;
    public TextMeshProUGUI Select_Take;
    public TextMeshProUGUI Select_Back;
    public TextMeshProUGUI HPinfo;
    public TextMeshProUGUI HP;

    public TextMeshProUGUI GoSettingsText;
    public TextMeshProUGUI Settings_Name;
    public TextMeshProUGUI Settings_EffectVol;
    public TextMeshProUGUI Settings_MusicVol;
    public TextMeshProUGUI Settings_InputSensivity;
    public TextMeshProUGUI Settings_InputTypeName;
    public TextMeshProUGUI Settings_Vibration;

    public TextMeshProUGUI Settings_Language;
    public TextMeshProUGUI Settings_Back;

    public GameObject StoreSelect;
    public GameObject AboutCar;
    public Slider StoreAcceleration;
    public Slider StoreMaxSpeed;
    public Slider StoreDifficulty;
    public TextMeshProUGUI Store_CarDescription;
    private bool OnStoreSelect;
    private int SelectedStore;
    public TextMeshProUGUI GoStoreText;
    public TextMeshProUGUI Store_Name;
    public TextMeshProUGUI Store_AccelerationText;
    public TextMeshProUGUI Store_MaxSpeedText;
    public TextMeshProUGUI Store_DriftText;
    public TextMeshProUGUI Store_Buy;
    public TextMeshProUGUI Store_Take;
    public TextMeshProUGUI Store_MiniDescription;
    public TextMeshProUGUI Store_Description;
    public TextMeshProUGUI Store_Back;
    public TextMeshProUGUI Store_Restore;
    public IAPButton BuyButton;
    public GameObject TakeButton;
    public GameObject AdsNoBought;
    public GameObject AdsBought;

    private bool DontChangeColor;

    public Transform[] CarsPos;
    private Car[] Cars;
    private Button[] LockText;

    public TextMeshProUGUI CarMoney;
    public TextMeshProUGUI CarCollected;
    public TextMeshProUGUI RecordText;
    public TextMeshProUGUI TrackMoney;
    public TextMeshProUGUI TrackCollected;
    public TextMeshProUGUI SpecialMoney;
    public TextMeshProUGUI SpecialCollected;

    public GameObject RateTable;
    public TextMeshProUGUI RateGame;
    public TextMeshProUGUI NoThanks;
    public Image[] Stars;

    private Vector3 prevCamPosition;
    private float prevCamSize;
    private Coroutine Forward;
    private Coroutine Backward;

    private int SelectedCar;
    private bool OnCarSelect;

    public Transform[] TrackPos;
    public Transform[] StorePos;
    public Image Fade;
    private int SelectedTrack;
    private bool OnTrackSelect;

    public Slider MusicVol;
    public Slider EffectVol;
    public TMP_Dropdown InputType;
    public TMP_Dropdown LanguageType;
    public TextMeshProUGUI[] Tips;
    

    private bool Buying;
    public Camera Cam;
    public Animator anim;
    public Animator NameAnim;
    public AudioSystem AudioSys;
    public LanguagesSystem LanguagesSystem;
    public IAPListener Listener;

    private void Awake()
    {
        InitializeAds();
    }
    private void Start()
    {
        StartCoroutine(OnStart());
        if (!GameData.AppHadOpened)
        {
            anim.Play("FirstStart");
            GameData.AppHadOpened = true;
            GameData.SessionStartTime = Time.time;
        }

        Game.Load();
        LoadLanguage();
        CheckOpenedCar();
        StartTake(GameData.PrevParking);
        AudioSys.PlayMusic("MusicMenu", 0f);
        LoadTips();
        if (!GameData.NoAds)
        {
            Game.Ads.CreateBanner(false);
        }
    }
    private IEnumerator OnStart()
    {
        yield return new WaitForFixedUpdate();
        if (!GameData.GameStarted)
        {
            FirstLoadSelect.gameObject.SetActive(true);
        }
        else
        {
            OfferRateGame();
        }
        yield break;
    }

    public void InitializeAds()
    {
        //MobileAds.Initialize(initStatus => { });
    }

    public void SetUpReady()
    {
        FirstLoadSelect.gameObject.SetActive(false);
        Game.Save();
        GoTrack();
        StartCoroutine(LoadScene(6, 1f));
    }

    public void OfferRateGame()
    {
        if (!GameData.GameRated && GameData.PlayedTime > 60 && Random.Range(0, 5) == 0)
        {
            RateGame.text = LanguagesSystem.Language.MainMenu.RateApp;
            NoThanks.text = LanguagesSystem.Language.MainMenu.NoThanks;
            anim.Play("RateGame");
            for (int i = 0; i < Stars.Length; i++)
            {
                Stars[i].color = new Color(0.7924528f, 0.7924528f, 0.7924528f, 1f);
            }
        }
    }
    public void OnStarClick(int a)
    {
        for (int i = 0; i <= a; i++)
        {
            Stars[i].color = new Color(1f, 0.8624264f, 0f, 1f);
        }
        GameData.GameRated = true;
        StartCoroutine(GoRateGame(1.5f));
        Game.Save();
    }
    public void OnNotRated()
    {
        anim.SetTrigger("RateGameEnd");
    }
    public IEnumerator GoRateGame(float duration)
    {
        yield return new WaitForSeconds(duration);
        anim.SetTrigger("RateGameEnd");
        yield return new WaitForSeconds(1f);
        Application.OpenURL("market://details?id=" + GameData.AppName);
        yield break;
    }

    public void OpenAll()
    {
        for(int i = 0; i < Game.Tracks.Length; i++)
        {
            Game.Tracks[i].opened = true;
        }
        for (int i = 0; i < Game.Cars.Length; i++)
        {
            Game.Cars[i].opened = true;
        }
    }
    public void LoadLanguage()
    {
        LanguagesSystem.LoadLanguage(GameData.Language);
    }
    public void UpdateLanguage()
    {
        GoTrackText.text = LanguagesSystem.Language.MainMenu.GoTrack;
        Track_Back.text = LanguagesSystem.Language.TrackSelect.Back;
        Track_BackToMenu.text = LanguagesSystem.Language.TrackSelect.Back;
        Track_Cost.text = LanguagesSystem.Language.TrackSelect.TrackCost;
        Track_Difficulity.text = LanguagesSystem.Language.TrackSelect.Difficulity;
        Track_MoneyRatio.text = LanguagesSystem.Language.TrackSelect.MoneyRatio;
        Track_Play.text = LanguagesSystem.Language.TrackSelect.Play;
        Track_Racord.text = LanguagesSystem.Language.TrackSelect.Record;

        GoSuperTrackText.text = LanguagesSystem.Language.MainMenu.GoSpecial;
        Super_Back.text = LanguagesSystem.Language.TrackSelect.Back;

        GoSelectText.text = LanguagesSystem.Language.MainMenu.GoCarSelect;
        Select_Acceleration.text = LanguagesSystem.Language.CarSelect.Acceleration;
        Select_Back.text = LanguagesSystem.Language.CarSelect.Back;
        Select_BackToMain.text = LanguagesSystem.Language.CarSelect.Back;
        Select_CarCost.text = LanguagesSystem.Language.CarSelect.CarCost;
        Select_Color.text = LanguagesSystem.Language.CarSelect.Color;
        Select_Description.text = "";
        Select_Drift.text = LanguagesSystem.Language.CarSelect.Drift;
        Select_MaxSpeed.text = LanguagesSystem.Language.CarSelect.MaxSpeed;
        Select_Take.text = LanguagesSystem.Language.CarSelect.Play;
        HP.text = LanguagesSystem.Language.CarSelect.HP;


        GoSettingsText.text = LanguagesSystem.Language.MainMenu.GoSettings;
        Settings_Back.text = LanguagesSystem.Language.Settings.Back;
        Settings_EffectVol.text = LanguagesSystem.Language.Settings.EffectVol;
        Settings_InputSensivity.text = LanguagesSystem.Language.Settings.InputSensivity;
        Settings_InputTypeName.text = LanguagesSystem.Language.Settings.InputTypeName;
        Settings_Vibration.text = LanguagesSystem.Language.Settings.Vibration;
        TMP_Dropdown.OptionData[] option = InputType.options.ToArray();
        for (int i = 0; i < option.Length; i++)
        {
            option[i].text = LanguagesSystem.Language.Settings.InputType[i];
        }
        InputType.itemText.text = LanguagesSystem.Language.Settings.InputType[InputType.value];
        Settings_MusicVol.text = LanguagesSystem.Language.Settings.MusicVol;
        Settings_Name.text = LanguagesSystem.Language.Settings.Settings;
        Settings_Language.text = LanguagesSystem.Language.Settings.Language;

        GoStoreText.text = LanguagesSystem.Language.MainMenu.GoStore;
        Store_Restore.text = LanguagesSystem.Language.StoreUi.Restore;
        Store_Back.text = LanguagesSystem.Language.Settings.Back;

        LanguageType.value = GameData.Language;

        LoadTips();
    }
    public void SetLanguage(int Language)
    {
        for (int i = 0; i < Flags.Length; i++)
        {
            Flags[i].color = new Color(0.5f, 0.5f, 0.5f);
        }
        GameData.Language = Language;
        Flags[Language].color = Color.white;
        LoadLanguage();
    }
    public void SetInput(int input)
    {
        for (int i = 0; i < InputImagine.Length; i++)
        {
            InputImagine[i].color = new Color(0.5f, 0.5f, 0.5f);
        }
        InputImagine[input].color = Color.white;
        GameData.InputType = (GameData.InputTypes)input;
    }
    public void OnLanguageChanged()
    {
        GameData.Language = LanguageType.value;
        Game.Save();
        LoadLanguage();
    }
    private void CheckOpenedCar()
    {
        Cars = new Car[CarsPos.Length];
        LockText = new Button[CarsPos.Length];

        for (int i = 0; i < CarsPos.Length; i++)
        {
            LockText[i] = CarsPos[i].GetChild(0).GetComponent<Button>();
            if (CarsPos[i].childCount > 1)
            {
                Destroy(CarsPos[i].GetChild(1).gameObject);
                if (GameData.PrevParking != i)
                {
                    Cars[i] = Instantiate(Game.GetCar(i).car, CarsPos[i].position, CarsPos[i].rotation, CarsPos[i]);
                    Cars[i].SetColor(Game.GetCar(i).color);
                    Cars[i].transform.SetSiblingIndex(1);
                }
                if (LockText[i].transform.childCount > 0)
                {
                    if (Game.GetCar(i).opened)
                    {
                        LockText[i].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else
                    {
                        LockText[i].transform.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                LockText[i].gameObject.SetActive(false);
            }
        }
        
    }

    public void SelectCar(int place)
    {
        if (OnCarSelect)
            return;
        OnCarSelect = true;
        SelectedCar = place;
        prevCamPosition = Cam.transform.position;
        prevCamSize = Cam.orthographicSize;

        Select_Name.text = LanguagesSystem.Language.Car[Game.GetCar(place).Index].Name;
        HPinfo.text = Game.GetCar(place).car.Hp.ToString() + " HP";
        Select_Name.color = GetRareColor((int)Game.GetCar(place).Rare);
        Cost.text = Game.GetCar(place).cost + "$";
        CarTerms.text = "";
        if (Game.GetCar(place).BuyCheck.TermType != TermsCheck.TermsTypes.Null && !Game.GetCar(place).BuyCheck.AllRight(Game) && !Game.GetCar(place).opened)
        {
            CarTerms.text = LanguagesSystem.Language.Car[Game.GetCar(place).Index].UnlockTerms;
            CarTerms.color = new Color(0.85f, 0.11f, 0.11f);
        }
        if (Game.GetCar(place).Premium)
        {
            CarTerms.text = LanguagesSystem.Language.Car[Game.GetCar(place).Index].UnlockTerms;
            CarTerms.color = new Color(0.94f, 0.87f, 0.27f);
        }
        Acceleration.value = Game.GetCar(place).car.Acceleration / (Game.GetCar(place).car.GeatChangeTime + 1);
        MaxSpeed.value = Game.GetCar(place).car.MaxSpeed;
        Difficulty.value = Game.GetCar(place).car.Acceleration * Game.GetCar(place).car.Slide * Mathf.Sqrt(Game.GetCar(place).car.TurnSpeed * Mathf.Sqrt(Game.GetCar(place).car.TurnRadius));
        Select_Description.text = LanguagesSystem.Language.Car[Game.GetCar(place).Index].Text;

        Forward = StartCoroutine(SelectGo(0));

        PlaySound("Click", 1f, false);
    }
    public void SelectBack()
    {
        OnCarSelect = false;
        Buying = false;

        Backward = StartCoroutine(SelectGoBack(0));
        PlaySound("ClickBack", 1f, false);
    }
    public void TakeCar(int i)
    {
        if (GameData.PrevParking == i)
            return;
        if (i == -1)
            i = SelectedCar;
        if (Game.GetCar(i).opened)
        {
            Cars[GameData.PrevParking] = Instantiate(Game.GetCar(GameData.PrevParking).car, CarsPos[GameData.PrevParking].position,
            CarsPos[GameData.PrevParking].rotation, CarsPos[GameData.PrevParking]);
            Cars[GameData.PrevParking].SetColor(Game.GetCar(GameData.PrevParking).color);
            LockText[GameData.PrevParking].gameObject.SetActive(true);
            LockText[i].gameObject.SetActive(false);

            GameData.CarNum = Game.GetCar(i).Index;
            GameData.PrevParking = i;
            Destroy(AnimCar.GetChild(0).gameObject);
            Car car = Instantiate(Game.GetCar(i).car, AnimCar);
            car.SetColor(Game.GetCar(i).color);

            if (CarsPos[i].childCount > 1)
            {
                Destroy(CarsPos[i].GetChild(1).gameObject);
                Cars[i] = null;
            }
            DontChangeColor = true;
            Rslider.value = Game.GetCar(i).color.r;
            Gslider.value = Game.GetCar(i).color.g;
            Bslider.value = Game.GetCar(i).color.b;
            DontChangeColor = false;

            anim.speed = Mathf.Sqrt(Mathf.Sqrt(car.Acceleration) / 0.4f);
            Select_Take.text = LanguagesSystem.Language.CarSelect.Play;
            PlaySound("Click", 1f, false);
            Backward = StartCoroutine(SelectGoBack(0));
        }
        else if (Buying)
        {
            BuyCar(i);
            Select_Take.text = LanguagesSystem.Language.CarSelect.Play;
        }
        else if (GameData.Money >= Game.GetCar(i).cost && Game.GetCar(i).BuyCheck.AllRight(Game))
        {
            Buying = true;
            Select_Take.text = LanguagesSystem.Language.TrackSelect.Sure;
            PlaySound("Click", 1f, false);
        }
        else if(GameData.Money < Game.GetCar(i).cost)
        {
            StartCoroutine(PrintNoMoney(0));
        }
        else
        {
            StartCoroutine(PrintCheckTerms(0));
        }
        Game.Save();

    }
    private void JustTakeCar(int index)
    {
        Cars[GameData.PrevParking] = Instantiate(Game.GetCar(GameData.PrevParking).car, CarsPos[GameData.PrevParking].position,
        CarsPos[GameData.PrevParking].rotation, CarsPos[GameData.PrevParking]);
        Cars[GameData.PrevParking].SetColor(Game.GetCar(GameData.PrevParking).color);
        LockText[GameData.PrevParking].gameObject.SetActive(true);
        LockText[Game.Cars[index].Parking].gameObject.SetActive(false);

        GameData.CarNum = index;
        GameData.PrevParking = Game.Cars[index].Parking;
        Destroy(AnimCar.GetChild(0).gameObject);
        Car car = Instantiate(Game.Cars[index].car, AnimCar);
        car.SetColor(Game.Cars[index].color);

        if (CarsPos[GameData.PrevParking].childCount > 1)
        {
            Destroy(CarsPos[GameData.PrevParking].GetChild(1).gameObject);
            Cars[GameData.PrevParking] = null;
        }
        DontChangeColor = true;
        Rslider.value = Game.GetCar(GameData.PrevParking).color.r;
        Gslider.value = Game.GetCar(GameData.PrevParking).color.g;
        Bslider.value = Game.GetCar(GameData.PrevParking).color.b;
        DontChangeColor = false;

        anim.speed = Mathf.Sqrt(Mathf.Sqrt(car.Acceleration) / 0.4f);
        Select_Take.text = LanguagesSystem.Language.CarSelect.Play;
        PlaySound("Click", 1f, false);
    }
    public void StartTake(int i)
    {
        Cars[GameData.PrevParking] = Instantiate(Game.GetCar(GameData.PrevParking).car, CarsPos[GameData.PrevParking].position,
        CarsPos[GameData.PrevParking].rotation, CarsPos[GameData.PrevParking]);
        Cars[GameData.PrevParking].SetColor(Game.GetCar(GameData.PrevParking).color);
        LockText[GameData.PrevParking].gameObject.SetActive(true);
        LockText[i].gameObject.SetActive(false);

        GameData.CarNum = Game.GetCar(i).Index;
        GameData.PrevParking = i;
        Destroy(AnimCar.GetChild(0).gameObject);
        Car car = Instantiate(Game.GetCar(i).car, AnimCar);
        car.SetColor(Game.GetCar(i).color);

        anim.speed = Mathf.Sqrt(Mathf.Sqrt(car.Acceleration) / 0.4f);
    }
    public void BuyCar(int i)
    {
        Buying = false;
        Game.GetCar(i).opened = true;
        LocalLock.SetActive(false);
        GameData.Money -= Game.GetCar(i).cost;
        Game.Save();

        UpdateMoneyAndCollected();
        PlaySound("Buy", 1f, false);
        Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 50));
    }
    public void ChangeCarColor(bool Yes)
    {
        if (DontChangeColor)
            return;
        Color color = new Color(Rslider.value, Gslider.value, Bslider.value, 0f);
        Game.Cars[GameData.CarNum].color = color;
        if (AnimCar.childCount > 0)
        {
            AnimCar.GetChild(0).GetComponent<Car>().SetColor(color);
        }
        Game.Save();
    }

    public void InitilizePurchase()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        UnityPurchasing.Initialize(this, builder);
    }
    public void UpdateStore()
    {
        AdsBought.SetActive(GameData.NoAds);
        AdsNoBought.SetActive(!GameData.NoAds);
    }
    public void SelectStore(int Index)
    {
        OnStoreSelect = true;
        SelectedStore = Index;
        prevCamPosition = Cam.transform.position;
        prevCamSize = Cam.orthographicSize;
        TakeButton.SetActive(Game.Stores[Index].Open);
        BuyButton.gameObject.SetActive(!Game.Stores[Index].Open);
        BuyButton.productId = Game.Stores[Index].id;

        switch (Game.Stores[SelectedStore].Types[0])
        {
            case StoreBuy.PurchaseType.Car:
                AboutCar.SetActive(true);
                Store_Description.gameObject.SetActive(false);
                Car car = Game.Cars[Game.Stores[SelectedStore].ProductIndex].car;
                Store_Name.text = LanguagesSystem.Language.Car[Game.Stores[SelectedStore].ProductIndex].Name;
                StoreAcceleration.value = car.Acceleration;
                StoreMaxSpeed.value = car.MaxSpeed;
                Store_AccelerationText.text = LanguagesSystem.Language.CarSelect.Acceleration;
                Store_MaxSpeedText.text = LanguagesSystem.Language.CarSelect.MaxSpeed;
                Store_DriftText.text = LanguagesSystem.Language.CarSelect.Drift;
                StoreDifficulty.value = car.Acceleration * car.Slide * car.Slide * Mathf.Sqrt(car.TurnRadius);
                Store_MiniDescription.text = LanguagesSystem.Language.Store[SelectedStore].MiniDescription;
                Store_CarDescription.text = LanguagesSystem.Language.Store[SelectedStore].Description;
                if (Game.Stores[SelectedStore].Open)
                {
                    Store_Take.text = LanguagesSystem.Language.StoreUi.TakeCar;
                }
                else
                {
                    Store_Buy.text = LanguagesSystem.Language.StoreUi.Buy + Game.Stores[SelectedStore].Cost + "$";
                }
                break;
            case StoreBuy.PurchaseType.NoAds:
                Store_Name.text = LanguagesSystem.Language.MainUIText.NoAds;
                AboutCar.SetActive(false);
                Store_Description.gameObject.SetActive(true);
                if (Game.Stores[SelectedStore].Open)
                {
                    Store_Take.text = LanguagesSystem.Language.StoreUi.Bought;
                }
                else
                {
                    Store_Buy.text = LanguagesSystem.Language.StoreUi.Buy + Game.Stores[SelectedStore].Cost + "$";
                }
                Store_Description.text = LanguagesSystem.Language.Store[SelectedStore].Description;
                Store_MiniDescription.text = LanguagesSystem.Language.Store[SelectedStore].MiniDescription;
                break;
            case StoreBuy.PurchaseType.Track:
                AboutCar.SetActive(false);
                Store_Description.gameObject.SetActive(true);
                if (Game.Stores[SelectedStore].Open)
                {
                    Store_Take.text = LanguagesSystem.Language.StoreUi.TakeTrack;
                }
                else
                {
                    Store_Buy.text = LanguagesSystem.Language.StoreUi.Buy + Game.Stores[SelectedStore].Cost + "$";
                }
                Store_Description.text = LanguagesSystem.Language.Store[SelectedStore].Description;
                Store_MiniDescription.text = LanguagesSystem.Language.Store[SelectedStore].MiniDescription;
                break;
        }
        Forward = StartCoroutine(SelectGo(2));
        PlaySound("Click", 1f, false);
    }
    public void SelectStoreBack()
    {
        OnStoreSelect = false;
        Backward = StartCoroutine(SelectGoBack(2));
        PlaySound("Click", 1f, false);
    }
    public void StoreTake()
    {
        switch(Game.Stores[SelectedStore].Types[0])
        {
            case StoreBuy.PurchaseType.Car:
                if (Game.Stores[SelectedStore].Open)
                {
                    JustTakeCar(Game.Stores[SelectedStore].ProductIndex);
                    SelectStoreBack();
                }
                break;
            case StoreBuy.PurchaseType.NoAds:
                if (Game.Stores[SelectedStore].Open)
                {
                    
                }
                break;
            case StoreBuy.PurchaseType.Track:
                break;
        }
    }
    public void PurchaseComplete(Product product)
    {
        for(int i = 0; i < Game.Stores.Length; i++)
        {
            if(Game.Stores[i].id == product.definition.id)
            {
                OnPaid(i);
            }
        }
    }
    public void PurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        if(reason == PurchaseFailureReason.DuplicateTransaction)
        {
            for (int i = 0; i < Game.Stores.Length; i++)
            {
                if (Game.Stores[i].id == product.definition.id)
                {
                    OnPaid(i);
                }
            }
        }
    }
    public void OnPaid(int Index)
    {
        Game.Stores[Index].BuyThis();
        switch(Game.Stores[Index].Types[0])
        {
            case StoreBuy.PurchaseType.Car:
                Store_Buy.text = LanguagesSystem.Language.StoreUi.TakeCar;
                break;
            case StoreBuy.PurchaseType.Track:
                Store_Buy.text = LanguagesSystem.Language.StoreUi.TakeTrack;
                break;
            case StoreBuy.PurchaseType.NoAds:
                GameData.NoAds = true;
                Store_Take.text = LanguagesSystem.Language.StoreUi.Bought;
                AdsBought.SetActive(true);
                AdsNoBought.SetActive(false);
                Game.Ads.HideBanner();
                break;
        }
        Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 50));
        PlaySound("Buy", 1f, false);
        BuyButton.gameObject.SetActive(false);
        TakeButton.SetActive(true);
        Game.Save();

    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        throw new System.NotImplementedException();
    }
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Product[] products = controller.products.all;
        for(int i = 0; i < Game.Stores.Length; i++)
        {
            for(int a = 0; a < products.Length; a++)
            {
                if(Game.Stores[i].id == products[a].definition.id)
                {
                    Game.Stores[i].Open = products[a].hasReceipt;
                    continue;
                }
            }
        }
    }


    public void SelectTrack(int place)
    {
        if (OnTrackSelect)
            return;
        OnTrackSelect = true;
        SelectedTrack = place;
        prevCamPosition = Cam.transform.position;
        prevCamSize = Cam.orthographicSize;

        Track_CostNum.text = Game.GetTrack(place).Cost.ToString() + "$";
        TrackTerms.text = "";
        if(Game.GetTrack(place).BuyCheck.TermType != TermsCheck.TermsTypes.Null && !Game.GetTrack(place).BuyCheck.AllRight(Game) && !Game.GetTrack(place).opened)
        {
            TrackTerms.text = LanguagesSystem.Language.Track[Game.GetTrack(place).Index].UnlockTerms;
            TrackTerms.color = new Color(0.85f, 0.11f, 0.11f);
        }
        Track_Name.text = LanguagesSystem.Language.Track[Game.GetTrack(place).Index].Name;
        RecordText.gameObject.SetActive(false);
        if (Game.GetTrack(place).TrackType == TrackSelect.TrackTypes.Time)
        {
            float Record = Game.GetTrack(place).Records[GameData.CarNum, Game.GetTrack(place).NowRecord[GameData.CarNum]];
            Track_Racord.text = Record != 0 ? LanguagesSystem.Language.TrackSelect.Record + TimeLapString(Record) : LanguagesSystem.Language.TrackSelect.NoTime;
            Track_Racord.gameObject.SetActive(true);
        }
        TrackDescription.text = LanguagesSystem.Language.Track[Game.GetTrack(place).Index].Text;
        TrackDifficulty.value = Game.GetTrack(place).Difficulty;
        TrackMoneyRatio.value = Game.GetTrack(place).MoneyRatio;

        Forward = StartCoroutine(SelectGo(1));
        PlaySound("Click", 1f, false);
    }
    public void SelectTrackBack()
    {
        OnTrackSelect = false;
        Buying = false;

        Backward = StartCoroutine(SelectGoBack(1));
        PlaySound("ClickBack", 1f, false);
    }
    public void TakeTrack(int i)
    {
        if(i == -1)
        {
            i = SelectedTrack;
        }
        if (Game.GetTrack(i).opened)
        {
            GameData.TrackNum = Game.GetTrack(i).Index;
            StartCoroutine(LoadScene(i, 0));
        }
        else if (Buying)
        {
            PlaySound("Click", 1f, false);
            BuyTrack(i);
            Track_Play.text = LanguagesSystem.Language.TrackSelect.Play;
        }
        else if (GameData.Money >= Game.GetTrack(i).Cost && Game.GetTrack(i).BuyCheck.AllRight(Game))
        {
            PlaySound("Click", 1f, false);
            Buying = true;
            Track_Play.text = LanguagesSystem.Language.TrackSelect.Sure;
        }
        else if(GameData.Money < Game.GetTrack(i).Cost)
        {
            StartCoroutine(PrintNoMoney(1));
        }
        else
        {
            StartCoroutine(PrintCheckTerms(1));
        }
        Game.Save();
    }
    private IEnumerator LoadScene(int a, float duration)
    {
        yield return new WaitForSeconds(duration);
        for(int i = 0; i < 20; i++)
        {
            Fade.color = new Color(0, 0, 0, (float)i / 18f);
            yield return new WaitForFixedUpdate();
        }
        AudioSys.StopMusic();
        SceneManager.LoadScene(Game.GetTrack(a).Scene);
        yield break;
    }
    public void BuyTrack(int i)
    {
        Buying = false;
        Game.GetTrack(i).opened = true;
        TrackLock.SetActive(false);
        TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        GameData.Money -= Game.GetTrack(i).Cost;
        Game.Save();

        UpdateMoneyAndCollected();
        PlaySound("Buy", 1f, false);
        Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 50));
    }
    public void UpdateMoneyAndCollected()
    {
        TrackMoney.text = GameData.Money.ToString() + " $";
        int All = 0;
        int Opened = 0;
        for (int i = 0; i < Game.Tracks.Length; i++)
        {
            if (!Game.Tracks[i].Special)
            {
                All++;
                if (Game.Tracks[i].opened)
                {
                    Opened++;
                }
            }
        }
        TrackCollected.text = Opened.ToString() + "/" + All.ToString();

        SpecialMoney.text = GameData.Money.ToString() + "$";
        int All1 = 0;
        int Opened1 = 0;
        for (int i = 0; i < Game.Tracks.Length; i++)
        {
            if (Game.Tracks[i].Special)
            {
                All1++;
                if (Game.Tracks[i].opened)
                {
                    Opened1++;
                }
            }
        }
        SpecialCollected.text = Opened1.ToString() + "/" + All1.ToString();

        CarMoney.text = GameData.Money.ToString() + "$";
        int All2 = 0;
        int Opened2 = 0;
        for (int i = 0; i < Game.Cars.Length; i++)
        {
            if (Game.Cars[i].Index == i)
            {
                All2++;
                if (Game.Cars[i].opened)
                {
                    Opened2++;
                }
            }
        }
        CarCollected.text = Opened2.ToString() + "/" + All2.ToString();
    }

    public void CheckSettings()
    {
        EffectVol.value = Mathf.RoundToInt(GameData.Volume * 5f);
        MusicVol.value = Mathf.RoundToInt(GameData.MusicVolume * 5f);
        InputType.value = (int)GameData.InputType;
        SensivitySlider.value = GameData.TurnSensivity;
        VibrationSlider.value = GameData.Vibration;
    }

    public void ChangeSensivity()
    {
        GameData.TurnSensivity = SensivitySlider.value;
    }
    public void ChangeInput()
    {
        GameData.InputType = (GameData.InputTypes)InputType.value;
        if (GameData.InputType == GameData.InputTypes.Buttons)
        {
            SensivitySlider.interactable = true;
        }
        else
        {
            SensivitySlider.interactable = false;
        }
        Game.Save();

        PlaySound("Click", 1f, false);
    }
    public void ChangeEffectVol()
    {
        GameData.Volume = EffectVol.value / 5f;
        Game.Save();

        PlaySound("Click", 1f, false);
    }
    public void ChangeMusicVol()
    {
        GameData.MusicTime = AudioSys.GetMusicTime();
        GameData.MusicVolume = MusicVol.value / 5f;
        Game.Save();
        AudioSys.PlayMusic("MusicMenu", GameData.MusicTime);
    }
    public void ChangeVibration()
    {
        if (VibrationSlider.value < 0.1f)
        {
            VibrationSlider.value = 0;
            GameData.Vibration = 0;
        }
        {
            GameData.Vibration = VibrationSlider.value;
        }
        
        
        Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 15));
        Game.Save();
    }

    public IEnumerator SelectGo(int Type)
    {
        if (Backward != null)
            StopCoroutine(Backward);
        switch(Type)
        {
            case 0: //CarSelect
                {
                    SelectCanvas.SetActive(true);
                    carSelect.SetActive(true);
                    anim.enabled = false;
                    LockText[SelectedCar].transform.GetChild(0).gameObject.SetActive(false);
                    if (Game.GetCar(SelectedCar).opened)
                    {
                        Select_Take.text = LanguagesSystem.Language.CarSelect.Play;
                        LocalLock.SetActive(false);
                    }
                    else if (Buying)
                    {
                        Select_Take.text = LanguagesSystem.Language.TrackSelect.Sure;
                        LocalLock.SetActive(true);
                    }
                    else
                    {
                        Select_Take.text = LanguagesSystem.Language.CarSelect.BuyCar;
                        LocalLock.SetActive(true);
                    }
                    SelectPlate.color = new Color(1, 1, 1, 0);
                    Vector3 Position = new Vector3(CarsPos[SelectedCar].position.x, CarsPos[SelectedCar].position.y, Cam.transform.position.z);
                    while ((Cam.transform.position - Position).magnitude > 0.01f)
                    {
                        Cam.transform.position = Vector3.Lerp(Cam.transform.position, Position, 0.05f);
                        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, 4, 0.075f);
                        SelectPlate.color = Color.Lerp(SelectPlate.color, new Color(1, 1, 1, 1), 0.05f);
                        yield return new WaitForFixedUpdate();
                    }
                }
                break;
            case 1://TrackSelect
                {
                    TrackLock.SetActive(false);
                    SelectCanvas.SetActive(true);
                    trackSelect.SetActive(true);
                    anim.enabled = false;
                    if (Game.GetTrack(SelectedTrack).opened)
                    {
                        Track_Play.text = LanguagesSystem.Language.TrackSelect.Play;
                        TrackLock.SetActive(false);
                    }
                    else if (Buying)
                    {
                        Track_Play.text = LanguagesSystem.Language.TrackSelect.Sure;
                        TrackLock.SetActive(true);
                    }
                    else
                    {
                        Track_Play.text = LanguagesSystem.Language.TrackSelect.BuyTrack;
                        TrackLock.SetActive(true);
                    }
                    SelectPlate.color = new Color(1, 1, 1, 0);
                    Vector3 Position = new Vector3(TrackPos[SelectedTrack].position.x, TrackPos[SelectedTrack].position.y, Cam.transform.position.z);
                    while ((Cam.transform.position - Position).magnitude > 0.01f)
                    {
                        Cam.transform.position = Vector3.Lerp(Cam.transform.position, Position, 0.05f);
                        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, 4, 0.075f);
                        SelectPlate.color = Color.Lerp(SelectPlate.color, new Color(1, 1, 1, 1), 0.05f);
                        yield return new WaitForFixedUpdate();
                    }
                }
                break;
            case 2://StoreSelect
                {
                    SelectCanvas.SetActive(true);
                    StoreSelect.SetActive(true);
                    anim.enabled = false;
                    SelectPlate.color = new Color(1, 1, 1, 0);
                    Vector3 Position = new Vector3(StorePos[SelectedStore].position.x, StorePos[SelectedStore].position.y, Cam.transform.position.z);
                    while ((Cam.transform.position - Position).magnitude > 0.01f)
                    {
                        Cam.transform.position = Vector3.Lerp(Cam.transform.position, Position, 0.05f);
                        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, 4, 0.075f);
                        SelectPlate.color = Color.Lerp(SelectPlate.color, new Color(1, 1, 1, 1), 0.05f);
                        yield return new WaitForFixedUpdate();
                    }
                }
                break;
        }
        yield break;
    }
    public IEnumerator SelectGoBack(int Type)
    {
        if (Forward != null)
            StopCoroutine(Forward);
        switch(Type)
        {
            case 0:
                {
                    carSelect.SetActive(false);
                    OnCarSelect = false;
                    if (!Game.GetCar(SelectedCar).opened)
                        LockText[SelectedCar].transform.GetChild(0).gameObject.SetActive(true);
                    while ((Cam.transform.position - prevCamPosition).magnitude > 0.01f)
                    {
                        Cam.transform.position = Vector3.Lerp(Cam.transform.position, prevCamPosition, 0.05f);
                        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, prevCamSize, 0.075f);
                        SelectPlate.color = Color.Lerp(SelectPlate.color, new Color(1, 1, 1, 0), 0.05f);
                        yield return new WaitForFixedUpdate();
                    }
                    anim.enabled = true;
                    SelectCanvas.SetActive(false);

                }
                break;
            case 1:
                {
                    trackSelect.SetActive(false);
                    OnTrackSelect = false;
                    TrackLock.SetActive(false);
                    while ((Cam.transform.position - prevCamPosition).magnitude > 0.01f)
                    {
                        Cam.transform.position = Vector3.Lerp(Cam.transform.position, prevCamPosition, 0.05f);
                        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, prevCamSize, 0.075f);
                        SelectPlate.color = Color.Lerp(SelectPlate.color, new Color(1, 1, 1, 0), 0.05f);
                        yield return new WaitForFixedUpdate();
                    }
                    anim.enabled = true;
                    SelectCanvas.SetActive(false);

                }
                break;
            case 2:
                {
                    StoreSelect.SetActive(false);
                    OnStoreSelect = false;
                    while ((Cam.transform.position - prevCamPosition).magnitude > 0.01f)
                    {
                        Cam.transform.position = Vector3.Lerp(Cam.transform.position, prevCamPosition, 0.05f);
                        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, prevCamSize, 0.075f);
                        SelectPlate.color = Color.Lerp(SelectPlate.color, new Color(1, 1, 1, 0), 0.05f);
                        yield return new WaitForFixedUpdate();
                    }
                    anim.enabled = true;
                    SelectCanvas.SetActive(false);
                }
                break;
        }

        yield break;
    }
    public IEnumerator PrintNoMoney(int i)
    {
        PlaySound("NoMoney", 1f, false);
        switch (i)
        {
            case 0:
                Select_Take.text = LanguagesSystem.Language.TrackSelect.NoMoney;
                Buying = false;
                yield return new WaitForSeconds(2.5f);
                Select_Take.text = LanguagesSystem.Language.TrackSelect.BuyTrack;
                break;
            case 1:
                Track_Play.text = LanguagesSystem.Language.TrackSelect.NoMoney;
                Buying = false;
                yield return new WaitForSeconds(2.5f);
                Track_Play.text = LanguagesSystem.Language.TrackSelect.BuyTrack;
                break;
        }
        yield break;
    }
    public IEnumerator PrintCheckTerms(int i)
    {
        PlaySound("NoMoney", 1f, false);
        switch (i)
        {
            case 0:
                Select_Take.text = LanguagesSystem.Language.TrackSelect.TrackTerms;
                Buying = false;
                yield return new WaitForSeconds(2.5f);
                Select_Take.text = LanguagesSystem.Language.CarSelect.BuyCar;
                break;
            case 1:
                Track_Play.text = LanguagesSystem.Language.TrackSelect.TrackTerms;
                Buying = false;
                yield return new WaitForSeconds(2.5f);
                Track_Play.text = LanguagesSystem.Language.TrackSelect.BuyTrack;
                break;
        }
        yield break;
    }

    public void LoadTips()
    {
        for(int i = 0; i < Tips.Length; i++)
        {
            int lenght = LanguagesSystem.Language.MainMenu.Tips.Length;
            string Tip = LanguagesSystem.Language.MainMenu.Tips[Random.Range(0, lenght)];
            Tips[i].text = Tip;
            Tips[i].gameObject.SetActive(false);
        }
        Tips[Random.Range(0, Tips.Length)].gameObject.SetActive(true);
    }

    public void GoTrack()
    {
        if (Navightion != MenuNavightion.Idle)
            return;
        anim.SetTrigger("GoTrack");
        Navightion = MenuNavightion.Track;
        GameData.MenuNavigation = (int)Navightion;
        for (int i = 0; i < TrackPos.Length; i++)
        {
            if (Game.GetTrack(i) != null)
            {
                TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = LanguagesSystem.Language.Track[Game.GetTrack(i).Index].Name;
                if (Game.GetTrack(i).opened)
                {
                    TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                }
                else
                {
                    TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.grey;
                }
                if (TrackPos[i].childCount > 1)
                {
                    Color color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 0);
                    TrackPos[i].GetChild(1).GetComponent<Car>().SetColor(color);
                }
            }
        }
        TrackMoney.text = GameData.Money.ToString() + " $";
        int All = 0;
        int Opened = 0;
        for (int i = 0; i < Game.Tracks.Length; i++)
        {
            if (!Game.Tracks[i].Special)
            {
                All++;
                if (Game.Tracks[i].opened)
                {
                    Opened++;
                }
            }
        }
        TrackCollected.text = Opened.ToString() + "/" + All.ToString();

        PlaySound("GoToTrack", 1f, false);
    }
    public void GoSpecial()
    {
        if (Navightion != MenuNavightion.Idle)
            return;
        anim.SetTrigger("GoSpecial");
        Navightion = MenuNavightion.Special;
        GameData.MenuNavigation = (int)Navightion;
        for (int i = 0; i < TrackPos.Length; i++)
        {
            if (Game.GetTrack(i) != null)
            {
                TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = LanguagesSystem.Language.Track[Game.GetTrack(i).Index].Name;
                if (Game.GetTrack(i).opened)
                {
                    TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                }
                else
                {
                    TrackPos[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.grey;
                }

                if (TrackPos[i].childCount > 1)
                {
                    Color color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 0);
                    TrackPos[i].GetChild(1).GetComponent<Car>().SetColor(color);
                }
            }
        }
        SpecialMoney.text = GameData.Money.ToString() + "$";
        int All = 0;
        int Opened = 0;
        for (int i = 0; i < Game.Tracks.Length; i++)
        {
            if (Game.Tracks[i].Special)
            {
                All++;
                if(Game.Tracks[i].opened)
                {
                    Opened++;
                }
            }
        }
        SpecialCollected.text = Opened.ToString() + "/" + All.ToString();

        PlaySound("GoToTrack", 1f, false);
    }
    public void GoCarSelect()
    {
        if (Navightion != MenuNavightion.Idle)
            return;
        anim.SetTrigger("GoCarSelect");
        Navightion = MenuNavightion.CarSelect;
        GameData.MenuNavigation = (int)Navightion;
        CheckOpenedCar();
        CarMoney.text = GameData.Money.ToString() + "$";
        int All = 0;
        int Opened = 0;
        for (int i = 0; i < Game.Cars.Length; i++)
        {
            if (Game.Cars[i].Index == i && !Game.Cars[i].Premium)
            {
                All++;
                if (Game.Cars[i].opened)
                {
                    Opened++;
                }
            }
        }
        CarCollected.text = Opened.ToString() + "/" + All.ToString();
        PlaySound("GoToTrack", 1f, false);
    }
    public void GoSettings()
    {
        if (Navightion != MenuNavightion.Idle)
            return;
        anim.SetTrigger("GoSettings");
        Navightion = MenuNavightion.Settings;
        CheckSettings();
        if(GameData.InputType == GameData.InputTypes.Buttons)
        {
            SensivitySlider.interactable = true;
        }
        else
        {
            SensivitySlider.interactable = false;
        }
        PlaySound("GoToTrack", 1f, false);
    }
    public void GoStore()
    {
        if (Navightion != MenuNavightion.Idle)
            return;
        anim.SetTrigger("GoStore");
        CheckOpenedCar();
        UpdateStore();
        Navightion = MenuNavightion.Store;
        PlaySound("GoToTrack", 1f, false);
    }

    public void Back()
    {
        switch (Navightion)
        {
            case MenuNavightion.Idle:
                break;
            case MenuNavightion.Special:
                anim.SetTrigger("SpecialBack");
                break;
            case MenuNavightion.CarSelect:
                anim.SetTrigger("CarSelectBack");
                break;
            case MenuNavightion.Settings:
                anim.SetTrigger("SettingsBack");
                break;
            case MenuNavightion.Track:
                anim.SetTrigger("TrackBack");
                break;
            case MenuNavightion.Store:
                anim.SetTrigger("StoreBack");
                break;
        }
        Navightion = MenuNavightion.Idle;
        GameData.MenuNavigation = 0;

        PlaySound("GoToTrack", 1f, false);
    }

    public void PlaySound(string name, float Vol, bool Rand)
    {
        if (AudioSys.GetSound(name) == null)
            return;
        AudioSource Source = gameObject.AddComponent<AudioSource>();
        Sound sound = AudioSys.GetSound(name);
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

    private void OnApplicationQuit()
    {
        Game.Save();
    }
    private void FixedUpdate()
    {

    }

}
