using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitMovement : MonoBehaviour
{
    public abstract Transform Target { get; set; }
    protected abstract void MoveUnitToTarget();
    //protected abstract void UpdateTravelVelocity()
}
