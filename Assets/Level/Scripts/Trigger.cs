using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public enum ActionType {Finish, Check, GoTrack, Delivery, GoCarSelect, Restart, Exit}
    public ActionType Action;
    public int Var;
    public bool Active;
    public Level level;
    public Delivery delivery;
    private GameObject EnteredCar;
    private void Start()
    {
        if (level == null)
            level = Level.active;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Car" && collision.gameObject != EnteredCar)
        {
            EnteredCar = collision.gameObject;
            switch(Action)
            {
                case ActionType.Exit:
                    level.Exit();
                    break;
                case ActionType.GoCarSelect:
                    level.GoCarSelect();
                    break;
                case ActionType.GoTrack:
                    level.GoTrack(Var);
                    break;
                case ActionType.Restart:
                    level.Restart();
                    break;
                case ActionType.Finish:
                    level.OverlapFinish();
                    break;
                case ActionType.Check:
                    level.OverlapCheck(Var);
                    break;
                case ActionType.Delivery:
                    if(Active)
                    {
                        level.OnDeliveryEnter();
                        level.AddHpProcent(0.25f);
                        delivery.MakeBox();
                        Active = false;
                    }
                    break;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Car")
        {
            EnteredCar = null;
        }
    }
}
