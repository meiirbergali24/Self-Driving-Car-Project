using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Car")]
public class CarData : ScriptableObject
{
    public float angle;
    public string label;
    public string lane;

    public void SetLane(string lane)
    {
        this.lane = lane;
    }

    public void SetLabel(string label)
    {
        this.label = label;
    }

    public void SetAngle(float angle)
    {
        this.angle = angle;
    }
}
