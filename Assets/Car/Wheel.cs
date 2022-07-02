using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public int num;
    private Car car;

    private void Awake()
    {
        car = transform.parent.GetComponent<Car>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Track")
        {
            car.WheelSurface[num] = Car.GroundType.Track;
        }
        else if (collision.tag == "TrackDirt")
        {
            car.WheelSurface[num] = Car.GroundType.TrackDirt;
        }
        else if (collision.tag == "Ground")
        {
            car.WheelSurface[num] = Car.GroundType.Ground;
        }
        else if (collision.tag == "Sand")
        {
            car.WheelSurface[num] = Car.GroundType.Sand;
        }
        else if (collision.tag == "Puddle")
        {
            car.WheelSurface[num] = Car.GroundType.Puddle;
        }
    }
}
