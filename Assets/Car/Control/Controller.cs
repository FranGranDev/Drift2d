using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;
using SimpleInputNamespace;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public Car car;
    public FixedJoystick Slider;
    public SteeringWheel Wheel;
    public GameObject Buttons;
    public AnimationCurve Curve;
    private float Acceleration;
    private bool Right;
    private bool Left;
    private float Turning;
    public float Turn;
    private bool Gas;
    private bool Brake;
    private bool isDriftBrake;
    private bool BrakeTap;
    private float TimeScale = 1f;
    private float TimeOffset;
    private float Offset = 0.15f;
    private float Const()
    {
        return Time.fixedDeltaTime * 100f;
    }
    private const float WheelRatio = 0.6467f;
    private delegate void TurnType();
    private TurnType TurningType;

    public Image GazPedal;
    public Image BrakePedal;
    public Sprite[] GazImagine;
    public Sprite[] BrakeImagine;
    public Image[] TurnSprites;

    public void GasTurn(bool On)
    {
        Gas = On;
        GazPedal.sprite = BrakeImagine[On ? 0 : 1];

        if (On)
        {
            Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 10));
            BrakeTap = false;
            isDriftBrake = false;
        }
           
    }
    public void BrakeTurn(bool On)
    {
        Brake = On;
        BrakePedal.sprite = GazImagine[On ? 0 : 1];
        if (BrakeTap && On && TimeOffset > Time.time)
        {
            TimeOffset = 0f;
            isDriftBrake = true;
        }
        if(!BrakeTap && On)
        {
            TimeOffset = Time.time + 0.25f;
            BrakeTap = true;
        }
        if(!On)
        {
            isDriftBrake = false;
        }
        else
        {
            Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 15));
        }
        
    }
    public void RightTurn(bool On)
    {
        Right = On;
        if (Right)
        {
            TurnSprites[1].color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            TurnSprites[1].color = new Color(0.6f, 0.6f, 0.6f);
        }
        if (On)
        {
            Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 5));
        }
    }
    public void LeftTurn(bool On)
    {
        Left = On;
        if (Left)
        {
            TurnSprites[0].color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            TurnSprites[0].color = new Color(0.6f, 0.6f, 0.6f);
        }

        if (On)
        {
            Vibration.CreateOneShot(Mathf.RoundToInt(GameData.Vibration * 5));
        }
    }

    public void Start()
    {
        //SetInput(GameData.InputType);
        
    }

    public void SetInput(GameData.InputTypes Type)
    {
        switch (GameData.InputType)
        {
            case GameData.InputTypes.Buttons:
                Buttons.SetActive(true);
                Slider.gameObject.SetActive(false);
                Wheel.gameObject.SetActive(false);
                TurningType = ButtonsTurn;
                break;
            case GameData.InputTypes.Slider:
                Buttons.SetActive(false);
                Slider.gameObject.SetActive(true);
                Wheel.gameObject.SetActive(false);
                TurningType = SliderTurn;
                break;
            case GameData.InputTypes.Wheel:
                Buttons.SetActive(false);
                Slider.gameObject.SetActive(false);
                Wheel.gameObject.SetActive(true);
                Wheel.maximumSteeringAngle = car.MaxWheelTurn;
                TurningType = WheelTurn;
                break;
        }
    }
    public void ScreenInput()
    {
        TurningType();
        if (Brake)
        {
            if (Acceleration > 0.1f)
                Acceleration = 0;
            if (!isDriftBrake)
            {
                Acceleration = Mathf.Lerp(Acceleration, -1, 0.05f);
            }
            else
            {
                Acceleration = -1.1f;
            }

        }
        else if (Gas)
        {
            if (Acceleration < -0.1f)
                Acceleration = 0;
            Acceleration = 1;
        }
        else
        {
            if (Acceleration > -0.1f || Acceleration < 0.1f)
                Acceleration = 0;
            Acceleration = Mathf.Lerp(Acceleration, 0, 0.5f);
        }
    }
    public void ButtonsTurn()
    {
        if (Right)
        {
            Turn = Mathf.Lerp(Turn, 1, (GameData.TurnSensivity) * 0.1f);
        }
        else if (Left)
        {
            Turn = Mathf.Lerp(Turn, -1, (GameData.TurnSensivity) * 0.1f);
        }
        else
        {
            Turn = 0;
        }

    }
    private void SliderTurn()
    {
        Turn = Slider.Direction.x;

    }
    private void WheelTurn()
    {
        Turn = Wheel.Value;
        
    }
    private void RecalculateTurn()
    {
        switch (GameData.InputType)
        {
            case GameData.InputTypes.Buttons:
                Turning = Turn * car.TurnRadius * 0.7f;
                break;
            case GameData.InputTypes.Slider:
                Turning = Turn * car.TurnRadius * 0.75f;
                break;
            case GameData.InputTypes.Wheel:
                Turning = Turn * car.TurnRadius * 0.8f;
                break;
        }
        
    }

    public void PC()
    {

        if (Input.GetKey(KeyCode.A))
        {
            Turn = Mathf.Lerp(Turn, -1, 0.2f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Turn = Mathf.Lerp(Turn, 1, 0.2f);
        }
        else
        {
            Turn = Mathf.Lerp(Turn, 0, 0.25f);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            GasTurn(true);
        }
        if(Input.GetKeyUp(KeyCode.W))
        {
            GasTurn(false);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            BrakeTurn(true);
        }
        if(Input.GetKeyUp(KeyCode.S))
        {
            BrakeTurn(false);
        }

        if (Brake)
        {
            if (Acceleration > 0.1f)
                Acceleration = 0;
            if(!isDriftBrake)
            {
                Acceleration = Mathf.Lerp(Acceleration, -1, 0.1f);
            }
            else
            {
                Acceleration = Mathf.Lerp(Acceleration, -1.1f, 0.5f);
            }
            
        }
        else if (Gas)
        {
            if (Acceleration < -0.1f)
                Acceleration = 0;
            Acceleration = Mathf.Lerp(Acceleration, 1, 0.075f);
        }
        else
        {
            if (Acceleration > -0.1f || Acceleration < 0.1f)
                Acceleration = 0;
            Acceleration = Mathf.Lerp(Acceleration, 0, 0.5f);
        }


    }
    private void Update()
    {
        //PC();
        ScreenInput();
    }
    private void FixedUpdate()
    {
        RecalculateTurn();
        car.Drive(Acceleration, Turning);
    }
}
