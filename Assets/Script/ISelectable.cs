using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable 
{
    public abstract IOrderable OrderableComp { get; }

    //public abstract GroupingSlotComp GroupingSlot { get; }

    public abstract GameObject Object { get; }
    public abstract ISelectable GetSelectable();
    public abstract void OnSelected();
    public abstract void DeSelect();
    
}
