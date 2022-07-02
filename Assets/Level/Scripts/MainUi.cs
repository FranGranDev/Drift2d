using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUi : MonoBehaviour
{
    public GameObject TimeTable;
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI Gear;
    public Image Speedo;
    public GameObject Printer;
    public TextMeshProUGUI LapTimeName;
    public TextMeshProUGUI LapTime;
    public TextMeshProUGUI FinalLapTime;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI CoolDown;
    public RectTransform MoneyTransform;
    public TextMeshProUGUI Money;
    public Image Hp;
    private bool HpOn;
    private float HpProcent;
    public TextMeshProUGUI Pause_Resume;
    public TextMeshProUGUI Pause_Restart;
    public TextMeshProUGUI Pause_TurnTrack;
    public TextMeshProUGUI Pause_MainMenu;
    public TextMeshProUGUI Revert_GetChance;

    public GameObject Tip;
    public TextMeshProUGUI Tip_Pause;
    public TextMeshProUGUI Tip_Unwind;
    public TextMeshProUGUI Tip_Brake;
    public TextMeshProUGUI Tip_Gaz;
    public TextMeshProUGUI Tip_Turn;

    public GameObject Bonus;
    public TextMeshProUGUI Bonus_Drift;
    public TextMeshProUGUI Bonus_Speed;
    public TextMeshProUGUI Bonus_Hp;
    public TextMeshProUGUI Bonus_No;

    public Transform PauseUi;
    public GameObject RevertButton;
    public Transform RevertUi;
    public TextMeshProUGUI ChanceText;

    private Coroutine LastShow;

    public string TimeLapString(float time)
    {
        string Minutes = Mathf.Floor(time / 59).ToString("00") + ",";
        string Seconds = (time - Mathf.Floor(time / 59) * 59).ToString("00.00");
        return (Minutes + Seconds);
    }
    public Animator anim;
    public GameObject Buttons;
    public GameObject Slider;
    public GameObject Wheel;
    public GameObject Gas;
    public GameObject Brake;

    private void Start()
    {
        Pause_MainMenu.text = LanguagesSystem.Language.MainUIText.MainMenu;
        Pause_Restart.text = LanguagesSystem.Language.MainUIText.Restart;
        Pause_Resume.text = LanguagesSystem.Language.MainUIText.Resume;
        Pause_TurnTrack.text = LanguagesSystem.Language.MainUIText.TurnMap;
        Revert_GetChance.text = LanguagesSystem.Language.MainUIText.StepBack;
        LapTimeName.text = LanguagesSystem.Language.MainUIText.LapTime;
    }

    public void PrintForTime(string text, float time)
    {
        if (LastShow != null)
        {
            StopCoroutine(LastShow);
        }
        LastShow = StartCoroutine(PrintText(text, time));
    }
    private IEnumerator PrintText(string text, float time)
    {
        anim.Play("PrintStart");
        Text.text = text;
        yield return new WaitForSeconds(time);
        anim.Play("PrintEnd");
        yield break;
    }

    public void TurnLapTime(bool On)
    {
        TimeTable.SetActive(On);
        anim.SetBool("TimerOn", On);
    }
    public void SetLapTime(float time)
    {
        LapTimeName.text = LanguagesSystem.Language.MainUIText.LapTime;
        LapTime.text = TimeLapString(time);
    }
    public void ShowLapTime(int type, float time)
    {
        FinalLapTime.text = TimeLapString(time);
        if (type == 0)
        {
            anim.SetTrigger("NewTrackRecord");
        }
        else if(type == 1)
        {
            anim.SetTrigger("BestLapTime");
        }
        else
        {
            anim.SetTrigger("LapTime");
        }
        
    }
    public void InvalidTime()
    {
        PrintForTime(LanguagesSystem.Language.MainUIText.InvalidTime, 2f);
        SetLapTime(0f);
    }
    public void BreakTime(float time)
    {
        string text = "";
        if (time != 9999f)
        {
            text = LanguagesSystem.Language.MainUIText.BrakeYourRecord + " " + TimeLapString(time);
        }
        else
        {
            text = LanguagesSystem.Language.MainUIText.SetFirstRecord;
        }
        PrintForTime(text, 2f);
    }
    public void SetSurvivedLap(int Survived, int Challange)
    {
        LapTimeName.text = LanguagesSystem.Language.MainUIText.Survived;
        LapTime.text = Survived.ToString() + " / " + Challange.ToString() + " " + LanguagesSystem.Language.MainUIText.Laps;
    }
    public void SetDeliveryLap(int Collected, int Challange, float time)
    {
        LapTimeName.text = Collected.ToString() + "/" + Challange + LanguagesSystem.Language.MainUIText.Delivered;
        LapTime.text = TimeLapString(time);
    }
    public void SetDriftScore(float Score, int Challange)
    {
        LapTimeName.text = LanguagesSystem.Language.MainUIText.DriftScore;
        LapTime.text = Score.ToString("0.0") + " / " + Challange.ToString();
    }
    public void SetBattleRoyaleScore(int Surived, int All)
    {
        LapTimeName.text = " ";
        LapTime.text = Surived.ToString() + " / " + All.ToString() + " Alive";
    }
    public void SetCoolDown(int Num)
    {
        if (Num == 0)
            CoolDown.text = LanguagesSystem.Language.MainUIText.GO;
        else
            CoolDown.text = Num.ToString();
    }
    public void TurnCoolDown(bool On)
    {
        CoolDown.gameObject.SetActive(On);
    }
    public void SetGear(int x)
    {
        if(x == -1)
        {
            Gear.text = "R";
        }
        else if(x == 0)
        {
            Gear.text = "N";
        }
        else
        {
            Gear.text = x.ToString();
        }
    }
    public void SetSpeed(float Power)
    {
        Speed.text = Mathf.Floor(Power * 350).ToString();
        Speedo.fillAmount = Power / 1.2f * 1.75f * 0.671f;
    }
    public void GetMoney(int value)
    {
        Money.text = value.ToString() + '$';
        MoneyTransform.localRotation = new Quaternion(0, 0, MoneyTransform.localRotation.z * (Random.Range(0, 2) == 0 ? -1 : 1), MoneyTransform.localRotation.w);
        MoneyTransform.localPosition = new Vector2(MoneyTransform.localPosition.x * (Random.Range(0, 2) == 0 ? -1 : 1), Random.Range(-130f, 130f));
        anim.SetTrigger("GetMoney");
    }
    public void WOW()
    {
        anim.SetTrigger("WOW");
    }
    public void SetRevertButton(bool On)
    {
        RevertButton.SetActive(On);
    }
    public void TurnHp(bool On)
    {
        Hp.transform.parent.gameObject.SetActive(On);
        HpOn = On;
        if(On)
        {
            StartCoroutine(HpMove());
        }
    }
    private IEnumerator HpMove()
    {
        while(HpOn)
        {
            Hp.fillAmount = Mathf.Lerp(Hp.fillAmount, HpProcent, Time.deltaTime * 2f);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
    public void SetHp(float Procent)
    {
        HpProcent = Procent;

    }
    public void CrushCar()
    {
        PrintForTime(LanguagesSystem.Language.MainUIText.CarCrushed, 2f);
    }
    
    public void TurnTip(bool on)
    {
        if (on)
        {
            Tip_Brake.text = LanguagesSystem.Language.MainUIText.Tip_Brake;
            Tip_Gaz.text = LanguagesSystem.Language.MainUIText.Tip_Gaz;
            Tip_Pause.text = LanguagesSystem.Language.MainUIText.Tip_Pause;
            Tip_Turn.text = LanguagesSystem.Language.MainUIText.Tip_Turn;
            Tip_Unwind.text = LanguagesSystem.Language.MainUIText.Tip_Unwind;
            anim.Play("Tip");
        }
        else
        {
            anim.SetTrigger("TipEnd");
        }
    }
    public void TurnBonus(bool on)
    {
        if (on)
        {
            Bonus_Drift.text = LanguagesSystem.Language.MainUIText.Slide;
            Bonus_Speed.text = LanguagesSystem.Language.MainUIText.Speed;
            Bonus_Hp.text = LanguagesSystem.Language.MainUIText.Hp;
            Bonus_No.text = LanguagesSystem.Language.MainMenu.NoThanks;
            anim.Play("Bonus");
        }
        else
        {
            anim.SetTrigger("BonusEnd");
        }
    }

    public void RevertTurn(bool On)
    {
        RevertUi.gameObject.SetActive(On);
        if(On)
        {
            ChanceText.text = LanguagesSystem.Language.MainUIText.StepBack;
        }
    }
    public void UpdateChanceButton()
    {
        ChanceText.text = LanguagesSystem.Language.MainUIText.StepBack;
    }
}
