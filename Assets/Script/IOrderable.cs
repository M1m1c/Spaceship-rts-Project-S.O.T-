using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOrderable
{
    //TODO change this from transform to aoibject that contains a transform and a order type
    public abstract Transform TargetOrderBeacon { get; set; }
    public abstract Transform RootTransform { get; }
}
