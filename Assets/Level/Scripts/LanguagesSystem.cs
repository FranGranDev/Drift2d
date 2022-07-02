using UnityEngine;
using System.Collections;
using System.IO;
using System.Resources;

public class LanguagesSystem : MonoBehaviour
{
    private static string Json;
    private static string NowLanguage = "en_US";
    public static string[] LanguagesArray = { "en_US", "ru_RU", "by_BY", "zh_CN"};
    public static LanguageData Language = new LanguageData();
    public Menu callback;

    private void OnEnable()
    {

    }

    public void LoadLanguage(int Num)
    {
        if (Num > LanguagesArray.Length)
        {
            GameData.Language = 0;
            NowLanguage = LanguagesArray[0];
            Debug.Log("Missing Language. Set to English");
        }
        else
        {
            GameData.Language = Num;
            NowLanguage = LanguagesArray[Num];
        }
        string path = Application.streamingAssetsPath + "/Languages/" + NowLanguage + ".json";
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(LoadDataAndroid(path));
#endif
#if UNITY_EDITOR
        LoadDataPC(path);
#endif
    }
    private void LoadDataPC(string Path)
    {
        Json = File.ReadAllText(Path);
        Language = JsonUtility.FromJson<LanguageData>(Json);
        callback.UpdateLanguage();
    }
    private IEnumerator LoadDataAndroid(string Path)
    {
        WWW www = new WWW(Path);
        yield return www;
        Language = JsonUtility.FromJson<LanguageData>(www.text);
        callback.UpdateLanguage();
    }
}

[System.Serializable]
public class LanguageData
{
    public MainMenuText MainMenu;
    public TrackSelectText TrackSelect;
    public CarSelectText CarSelect;
    public SettingText Settings;

    public Description[] Car;
    public Description[] Track;

    public StoreInfo[] Store;
    public StoreUi StoreUi;
    public MainUI MainUIText;
}

[System.Serializable] 
public struct MainMenuText
{
    public string GoTrack;
    public string GoSpecial;
    public string GoCarSelect;
    public string GoSettings;
    public string GoStore;
    public string[] Tips;
    public string RateApp;
    public string NoThanks;
}
[System.Serializable]
public struct TrackSelectText
{
    public string Unlock;
    public string NoTime;
    public string Difficulity;
    public string MoneyRatio;
    public string TrackTerms;
    public string Record;
    public string TrackCost;
    public string BuyTrack;
    public string NoMoney;
    public string Sure;
    public string Play;
    public string Back;
}
[System.Serializable]

public struct CarSelectText
{
    public string Acceleration;
    public string MaxSpeed;
    public string Drift;
    public string CarCost;
    public string Unlock;
    public string BuyCar;
    public string NoMoney;
    public string Play;
    public string Color;
    public string Back;
    public string HP;
}

[System.Serializable]
public struct SettingText
{
    public string Settings;
    public string EffectVol;
    public string MusicVol;
    public string InputSensivity;
    public string InputTypeName;
    public string[] InputType;
    public string Language;
    public string Back;
    public string Vibration;
}

[System.Serializable]
public struct Description
{
    public string Name;
    public string Text;
    public string UnlockTerms;
}
[System.Serializable]
public struct StoreInfo
{
    public string Name;
    public string Cost;
    public string Description;
    public string MiniDescription;
}
[System.Serializable]
public struct MainUI
{
    public string Resume;
    public string TurnMap;
    public string Restart;
    public string MainMenu;

    public string Grid;
    public string Speed;
    public string Slide;
    public string Bonus;
    public string Hp;
    public string No;

    public string Tip_Brake;
    public string Tip_Gaz;
    public string Tip_Pause;
    public string Tip_Unwind;
    public string Tip_Turn;
    public string NoAds;

    public string GO;
    public string GetChance;
    public string StepBack;

    public string SetFirstRecord;
    public string BrakeYourRecord;
    public string YoutRecord;

    public string InvalidTime;
    public string CarCrushed;

    public string Survived;
    public string Survive;
    public string LapTime;
    public string DriftScore;
    public string Lap;
    public string Laps;
    public string LapRoditelnuy;
    public string LeaveTrack_Die;

    public string DrinkStart;
    public string FreeStart;
    public string DriftStart;


    public string CarBattleWin;
    public string[] WaveNum;
    public string Wave;
    public string Done;

    public string DeliveryStart;
    public string Delivered;
    public string DeliveryEnd;
}
[System.Serializable] 
public struct StoreUi
{
    public string TakeCar;
    public string TakeTrack;
    public string Buy;
    public string Bought;
    public string Restore;
}
