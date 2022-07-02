using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static bool GameStarted;
    public static bool AppHadOpened;
    public static string AppName = "com.FranGran.RoyaleRacing";
    public static float MoneyRatio = 1;
    public CarSelect[] Cars;
    public TrackSelect[] Tracks;
    public StoreBuy[] Stores;
    public int LoadInRow;
    public int RevertInRow;
    public bool IncreaseLoadInRow()
    {
        if (LoadInRow >= 2)
        {
            LoadInRow = 0;
            Save();
            return true;
        }
        LoadInRow++;
        Save();
        return false;
    }
    public bool IncreaseReversInRow()
    {
        if (RevertInRow >= 1)
        {
            RevertInRow = 0;
            Save();
            return true;
        }
        RevertInRow++;
        Save();
        return false;
    }
    public float GetRecord()
    {
        if(Tracks[TrackNum].NowRecord[CarNum] > 0)
        {
            return Tracks[TrackNum].Records[CarNum, Tracks[TrackNum].NowRecord[CarNum]];
        }
        else
        {
            return 0f;
        }
    }
    public float GetRecord(int track)
    {
        float rec = 9999f;
        for(int i = 0; i < Cars.Length; i++)
        {
            if (Tracks[track].NowRecord[i] > 0 && Tracks[track].Records[i, Tracks[track].NowRecord[i]] < rec)
            {
                rec = Tracks[track].Records[i, Tracks[track].NowRecord[i]];
            }
        }
        return rec;
    }
    public float GetRecord(int track, int car)
    {
        if (Tracks[track].NowRecord[car] > 0)
        {
            return Tracks[track].Records[car, Tracks[track].NowRecord[car]];
        }
        else
        {
            return 9999f;
        }
    }
    public float GetMoneyRatio()
    {
        return Tracks[TrackNum].MoneyRatio * MoneyRatio;
    }
    public float GetCarMoneyRatio()
    {
        return Cars[CarNum].MoneyRatio;
    }
    public int GetNowRecord()
    {
        return Tracks[TrackNum].NowRecord[CarNum];
    }
    public int GetLapToWin()
    {
        if (Tracks[TrackNum].LapToWin.Length > CarNum)
            return Tracks[TrackNum].LapToWin[CarNum];
        else
            return 1;
    }
    public float GetDriftBest()
    {
        return Tracks[TrackNum].DriftBest[CarNum];
    }
    public int GetMaxWave()
    {
        if (Tracks[TrackNum].MaxWave.Length > CarNum)
            return Tracks[TrackNum].MaxWave[CarNum];
        else
            return 0;
    }
    public int GetBattleRoyalePlace(int car)
    {
        return Tracks[TrackNum].MaxBattleRoyalePlace[car];
    }
    public void UpdateBattleRoyale(int car, int place)
    {
        Tracks[TrackNum].MaxBattleRoyalePlace[car] = place;
    }

    public void OpenCar(int i)
    {
        Cars[i].opened = true;
    }

    public CarSelect GetCar(int Place)
    {
        for(int i = 0; i < Cars.Length; i++)
        {
            if (Cars[i].Parking == Place)
                return Cars[i];
        }
        return null;
    }
    public TrackSelect GetTrack(int Place)
    {
        for (int i = 0; i < Tracks.Length; i++)
        {
            if (Tracks[i].Place == Place)
                return Tracks[i];
        }
        return null;
    }
    public static bool isReverting;
    public static int Language;
    public static int MenuNavigation;
    public enum InputTypes { Buttons, Wheel, Slider};
    public static InputTypes InputType;
    public static float TurnSensivity;
    public static float Volume;
    public static float MusicVolume;
    public static float MusicTime;
    public static float Vibration;
    public static bool OnPause;
    public static int Money;
    public static int CarNum;
    public static int TrackNum;
    public static int PrevParking;
    public static bool NoAds;
    public AdMob Ads;
    public static bool GameRated;

    public static float PlayedTime;
    public static float SessionStartTime;
    public Color CarColor()
    {
        return Cars[GameData.CarNum].color;
    }

    public void Load()
    {
        SaveData data = SaveSystem.Load();
        #region PreLoad
        GameStarted = false;
        Language = 0;
        Money = 1000;
        CarNum = 0;
        TrackNum = 0;
        PrevParking = 0;

        Money = 5000;

        InputType = 0;
        MusicVolume = 1;
        Volume = 1;
        TurnSensivity = 0.5f;
        Vibration = 0.5f;

        for (int i = 0; i < Tracks.Length; i++)
        {
            Tracks[i].MaxBattleRoyalePlace = new int[Cars.Length];
            Tracks[i].Records = new float[Cars.Length, TrackSelect.MaxRecord];
            Tracks[i].DriftBest = new float[Cars.Length];
            Tracks[i].LapToWin = new int[Cars.Length];
            Tracks[i].MaxWave = new int[Cars.Length];
            Tracks[i].NowRecord = new int[Cars.Length];
            Tracks[i].Records = new float[Cars.Length, TrackSelect.MaxRecord];
            for (int a = 0; a < Cars.Length; a++)
            {
                Tracks[i].MaxBattleRoyalePlace[a] = -1;
                Tracks[i].LapToWin[a] = 1;
            }
        }
        #endregion
        if (data != null)
        {
            GameStarted = data.GameStart;
            Language = data.Language;
            Money = data.Money;
            CarNum = data.CarNum;
            TrackNum = data.TrackNum;
            PrevParking = data.PrevParking;
            TurnSensivity = data.TurnSensivity;
            LoadInRow = data.LoadInRow;
            RevertInRow = data.RevertInRow;
            NoAds = data.NoAds;
            PlayedTime = data.PlayedTime;
            GameRated = data.GameRated;
            Vibration = data.Vibration;
            for (int i = 0; i < Cars.Length; i++)
            {
                if (i > data.OpenCars.Length - 1)
                    break;
                Cars[i].opened = data.OpenCars[i];
                Cars[i].color = new Color(data.ColorCars[i, 0],
                data.ColorCars[i, 1], data.ColorCars[i, 2], data.ColorCars[i, 3]);
            }
            for(int i = 0; i < Tracks.Length; i++)
            {
                if (i > data.OpenTracks.Length - 1)
                {
                    break;
                }
                Tracks[i].opened = data.OpenTracks[i];
                Tracks[i].Records = new float[Cars.Length, TrackSelect.MaxRecord];
                Tracks[i].NowRecord = new int[Cars.Length];
                Tracks[i].MaxBattleRoyalePlace = new int[Cars.Length];
                for (int a = 0; a < Cars.Length; a++)
                {
                    for (int b = 0; b < TrackSelect.MaxRecord; b++)
                    {
                        Tracks[i].Records[a, b] = data.Records[i, a, b];
                    }
                    Tracks[i].MaxBattleRoyalePlace[a] = data.MaxBattleRoyalePlace[i, a];
                    Tracks[i].LapToWin[a] = data.LapToWin[i, a];
                    Tracks[i].MaxWave[a] = data.MaxWave[i, a];
                    Tracks[i].DriftBest[a] = data.DriftBest[i, a];
                    Tracks[i].NowRecord[a] = data.NowRecords[i, a];
                }     
            }
            for(int i = 0; i < Stores.Length; i++)
            {
                Stores[i].Open = data.StoreOpen[i];
            }

            InputType = (InputTypes)data.InputType;
            MusicVolume = data.MusicVol;
            Volume = data.EffectVol;
            MusicTime = data.MusicTime;
        }

        NoAds = true;
    }
    public void SavePlayedTime()
    {
        PlayedTime += Time.time - SessionStartTime;
        SessionStartTime = Time.time;
    }
    public void Save()
    {
        SavePlayedTime();
        SaveSystem.Save(this);
    }
    public void UpdateMaxWave(int Record)
    {
        Tracks[TrackNum].MaxWave[CarNum] = Record;
        Save();
    }
    public void UpdateDrift(float Record)
    {
        Tracks[TrackNum].DriftBest[CarNum] = Record;
        Save();
    }
    public void UpdateRecord(float Record)
    {
        Tracks[TrackNum].NowRecord[CarNum]++;
        Tracks[TrackNum].Records[CarNum, Tracks[TrackNum].NowRecord[CarNum]] = Record;
        Save();
    }
    public void UpdateLapToWin()
    {
        Tracks[TrackNum].LapToWin[CarNum]++;
        Save();
    }
}
[System.Serializable]
public class SaveData
{
    public bool GameStart;
    public int CarNum;
    public int TrackNum;
    public int PrevParking;
    public int Language;
    public int Money;
    public int LoadInRow;
    public int RevertInRow;
    public float[,,] Records;
    public int[,] NowRecords;
    public int[,] LapToWin;
    public float[,] TimeToWin;
    public int[,] TimeStage;
    public bool[] OpenCars;
    public bool[] OpenTracks;
    public float[,] ColorCars;
    public float[,] DriftBest;
    public int[,] MaxWave;
    public int[,] MaxBattleRoyalePlace;
    public bool[] StoreOpen;
    public bool NoAds;
    public float PlayedTime;
    public bool GameRated;
    public float Vibration;

    public float EffectVol;
    public float MusicVol;
    public int InputType;
    public float TurnSensivity;
    public float MusicTime;

    public SaveData(GameData data)
    {
        GameStart = GameData.GameStarted;
        Money = GameData.Money;
        CarNum = GameData.CarNum;
        TrackNum = GameData.TrackNum;
        PrevParking = GameData.PrevParking;
        TurnSensivity = GameData.TurnSensivity;
        LoadInRow = data.LoadInRow;
        RevertInRow = data.RevertInRow;
        Records = new float[data.Tracks.Length, data.Cars.Length, TrackSelect.MaxRecord];
        NowRecords = new int[data.Tracks.Length, data.Cars.Length];
        LapToWin = new int[data.Tracks.Length, data.Cars.Length];
        TimeToWin = new float[data.Tracks.Length, data.Cars.Length];
        TimeStage = new int[data.Tracks.Length, data.Cars.Length];
        DriftBest = new float[data.Tracks.Length, data.Cars.Length];
        MaxWave = new int[data.Tracks.Length, data.Cars.Length];
        OpenCars = new bool[data.Cars.Length];
        OpenTracks = new bool[data.Tracks.Length];
        ColorCars = new float[data.Cars.Length, 4];
        StoreOpen = new bool[data.Stores.Length];
        MaxBattleRoyalePlace = new int[data.Tracks.Length, data.Cars.Length];
        NoAds = GameData.NoAds;
        PlayedTime = GameData.PlayedTime;
        GameRated = GameData.GameRated;
        Vibration = GameData.Vibration;

        for (int i = 0; i < OpenCars.Length; i++)
        {
            OpenCars[i] = data.Cars[i].opened;
            ColorCars[i, 0] = data.Cars[i].color.r;
            ColorCars[i, 1] = data.Cars[i].color.g;
            ColorCars[i, 2] = data.Cars[i].color.b;
            ColorCars[i, 3] = data.Cars[i].color.a;
        }
        for (int i = 0; i < data.Tracks.Length; i++)
        {
            OpenTracks[i] = data.Tracks[i].opened;
            for (int a = 0; a < data.Cars.Length; a++)
            {
                for (int b = 0; b < TrackSelect.MaxRecord; b++)
                {
                    Records[i, a, b] = data.Tracks[i].Records[a, b];
                }
                NowRecords[i, a] = data.Tracks[i].NowRecord[a];
                LapToWin[i, a] = data.Tracks[i].LapToWin[a];
                DriftBest[i, a] = data.Tracks[i].DriftBest[a];
                MaxWave[i, a] = data.Tracks[i].MaxWave[a];
                MaxBattleRoyalePlace[i, a] = data.Tracks[i].MaxBattleRoyalePlace[a];
            }
        }
        for (int i = 0; i < data.Stores.Length; i++)
        {
            StoreOpen[i] = data.Stores[i].Open;
        }
        Language = GameData.Language;
        InputType = (int)GameData.InputType;
        EffectVol = GameData.Volume;
        MusicVol = GameData.MusicVolume;
        MusicTime = GameData.MusicTime;
    }
}

[System.Serializable]
public class CarSelect
{
    public string name;
    public string NeedToBuy;
    public string Description;
    public int Index;
    public Car car;
    public int Parking;
    public bool opened;
    public int cost;
    public bool Premium;
    [Range(0, 5)]
    public int Difficulty;
    [Range(1, 5)]
    public float MoneyRatio;
    public Color color;
    public enum RareType {Common, Rare, Epic, Legendary, Premium}
    public RareType Rare;

    public TermsCheck BuyCheck;
}
[System.Serializable]
public class TrackSelect
{
    public string name;
    public string NeedToBuy;
    public string Description;
    public enum TrackTypes {Time, Survive, Drift, Free, Drunk, Delivery};
    public TrackTypes TrackType;
    public bool opened;
    public int Index;
    public int Scene;
    public int Place;
    public int Cost;
    public bool Special;
    [Range(0, 5)]
    public int Difficulty;
    [Range(0, 5f)]
    public float MoneyRatio;
    public int[] LapToWin;
    public float[] DriftBest;
    public int[] MaxWave;
    public float[,] Records;
    public const int MaxRecord = 50;
    public int[] NowRecord;
    public int[] MaxBattleRoyalePlace;

    public TermsCheck BuyCheck;
}
[System.Serializable]
public class TermsCheck
{
    public enum TermsTypes {Null, OpenTrack, OpenCar, SetTime, SetTimeOnCar, NoWay, DoneWave, DriftPoints, OpenAllSpecialTracks, OpenAllTracks }
    public TermsTypes TermType;
    public int value;
    public int choice;
    public float floatValue;

    public bool AllRight(GameData game)
    {
        switch(TermType)
        {
            case TermsTypes.Null:
                return true;
            case TermsTypes.NoWay:
                return false;
            case TermsTypes.SetTimeOnCar:
                if(game.GetRecord(value, choice) < floatValue)
                {
                    return true;
                }
                break;
            case TermsTypes.DriftPoints:
                for(int i = 0; i < game.Cars.Length; i++)
                {
                    if (game.Tracks[value].DriftBest[i] >= floatValue)
                    {
                        return true;
                    }
                }
                break;
            case TermsTypes.SetTime:
                if (game.GetRecord(value) < floatValue)
                {
                    return true;
                }
                break;
            case TermsTypes.OpenCar:
                if (game.Cars[value].opened)
                    return true;
                break;
            case TermsTypes.OpenTrack:
                if (game.Tracks[value].opened)
                    return true;
                break;
            case TermsTypes.OpenAllSpecialTracks:
                for (int i = 0; i < game.Tracks.Length; i++)
                {
                    if (game.Tracks[i].Special)
                    {
                        if (!game.Tracks[i].opened)
                        {
                            return false;
                        }    
                    }
                }
                break;
            case TermsTypes.DoneWave:
                for(int i = 0; i < game.Cars.Length; i++)
                {
                    if (game.Tracks[value].MaxWave[i] >= choice)
                        return true;
                }
                break;
        }
        return false;
    }
}
[System.Serializable]
public class StoreBuy
{
    public string name;
    public string id;
    public bool Open;
    public int Index;
    public int ProductIndex;
    public enum PurchaseType {Car, NoAds, Track};
    public PurchaseType[] Types;
    public float Cost;

    public void BuyThis()
    {
        Open = true;
    }
}