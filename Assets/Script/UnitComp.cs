using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO this is supposed to function as a hub that can handle orders, both movement and attack orders
public class UnitComp : MonoBehaviour, IOrderable
{
    private Transform myOrderBeacon;
    public Transform TargetOrderBeacon 
    {
        get { return myOrderBeacon; } 
        set 
        { 
            myOrderBeacon = value; 
            MovementComp.TargetBeacon = value;
        } 
    }

    public UnitMovement MovementComp { get; private set; }

    void Start()
    {
        MovementComp = GetComponent<UnitMovement>();
    }

}
