using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class UnitMovement : MonoBehaviour
{
    public abstract Transform TargetBeacon { get; set; }

    public UnityEvent<bool> HasArrived = new UnityEvent<bool>(); 

    public virtual void SetAllowedToArrive(bool value)
    {
        allowedToArrive = value;
    }
    protected bool allowedToArrive = true;
    protected abstract void MoveUnitToTarget();
    //protected abstract void UpdateTravelVelocity()
}
