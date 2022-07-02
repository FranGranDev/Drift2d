using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Spectator : MonoBehaviour
{
    public Camera Cam;
    public static Spectator active;
    public FixedJoystick Joystick;
    [Range(1, 10f)]
    public float ZoomScale;
    private float StartSize;
    private float Height;
    private float Width;
    public float Speed;
    private float RelativeSize()
    {
        return Cam.orthographicSize / StartSize;
    }
    private float GetSpeed()
    {
        return Speed * RelativeSize();
    }
    private Vector3 StartPos;
    private Bounds bound;
    public GameObject[] UI;
    public Animator anim;
    public TextMeshProUGUI text;
    public void Awake()
    {
        active = this;
    }
    public void OnEnable()
    {
        StartPos = transform.position;
        ZoomScale = 1f;
        StartSize = Cam.orthographicSize;
        Height = StartSize * 2;
        Width = Height * Cam.aspect;
    }
    public void OnDisable()
    {

    }

    public void GoX(bool Right)
    {
        transform.position += new Vector3(Right ? GetSpeed() : -GetSpeed(), 0, 0);
    }
    public void GoY(bool Up)
    {
        transform.position += new Vector3(0, Up ? GetSpeed() : -GetSpeed(), 0);
    }
    public void Zoom(bool In)
    {
        Cam.orthographicSize += (In ? -35 : 35) * RelativeSize();

        if(Cam.orthographicSize < 5)
        {
            Cam.orthographicSize = 5;
        }
        if(Cam.orthographicSize > StartSize)
        {
            Cam.orthographicSize = StartSize;
        }
    }
    public void Back()
    {
        transform.position = StartPos;
        Cam.orthographicSize = StartSize;
    }

    public void Photo()
    {
        StartCoroutine(MakeScreenShot());
    }
    private IEnumerator MakeScreenShot()
    {
        for (int i = 0; i < UI.Length; i++)
        {
            UI[i].SetActive(false);
        }
        yield return new WaitForSecondsRealtime(0.1f);
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string fileName = "Screenshot" + timeStamp + ".png";
        string pathToSave = fileName;
        ScreenCapture.CaptureScreenshot(pathToSave);

        anim.Play("ScreenShotIdle");
        yield return new WaitForSecondsRealtime(0.02f);
        anim.Play("ScreenShot");
        yield return new WaitForSecondsRealtime(0.25f);
        for (int i = 0; i < UI.Length; i++)
        {
            UI[i].SetActive(true);
        }
    }
    public void Debug()
    {
        text.text = "Saved to: " + Application.persistentDataPath;
    }

    public void FixedUpdate()
    {

    }
    private void Update()
    {
        Vector3 NewPos = new Vector3(Joystick.Horizontal, Joystick.Vertical, 0);
        transform.position = Vector3.Lerp(transform.position, transform.position + NewPos * GetSpeed(), 0.1f);
    }
}
