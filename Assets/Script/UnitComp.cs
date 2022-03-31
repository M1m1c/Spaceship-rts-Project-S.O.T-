using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
            //if (!groupSlot.MyFormationGroup)
            //{
            //}
            //else
            //{
            //    groupSlot.MyFormationGroup.TargetOrderBeacon = value;
            //}
        }
    }

    //public FormationGroup MyFormationGroup { get; set; }

    public UnitMovement MovementComp { get; private set; }

    public Transform RootTransform => transform;

    public GroupingSlotComp GroupingSlot => groupSlot;

    private GroupingSlotComp groupSlot;

    void Start()
    {
        MovementComp = GetComponent<UnitMovement>();
        Assert.IsNotNull(MovementComp);

        groupSlot = GetComponent<GroupingSlotComp>();
        Assert.IsNotNull(groupSlot);
    }

}
