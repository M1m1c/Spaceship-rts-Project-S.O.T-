using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnit : MonoBehaviour, ISelectable
{
    private Color startcol;
    private Renderer myRenderer;

    public IOrderable OrderableComp { get; private set; }

    public GameObject Object => gameObject;

    public GroupingSlotComp GroupingSlot => groupSlot;

    private GroupingSlotComp groupSlot;

    void Start()
    {
        OrderableComp = GetComponentInParent<IOrderable>();
        groupSlot = GetComponentInParent<GroupingSlotComp>();

        myRenderer = gameObject.GetComponent<Renderer>();
        if (myRenderer) { startcol = myRenderer.material.color; }
    }

    public void OnSelected()
    {
        myRenderer.material.color = Color.green;
    }

    public void DeSelect()
    {
        myRenderer.material.color = startcol;
    }

    public ISelectable GetSelectable()
    {
        //TODO if part of group return group instead

        if (GroupingSlot.MyFormationGroup)
        {
            return GroupingSlot.MyFormationGroup;
        }
        else
        {
            return this;
        }
    }

}
