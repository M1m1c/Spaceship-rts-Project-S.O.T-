using System.Collections.Generic;
using UnityEngine;

//used for similar space ships to form formations and travel together
public class FormationGroup : MonoBehaviour, ISelectable, IOrderable
{
    public FormationSettings defaultFormation;
    public Transform orderBeaconPrefab;
    private FormationSettings currentFormation;
    private Dictionary<int, SelectableUnit> unitSelections = new Dictionary<int, SelectableUnit>();
    private Dictionary<int, UnitComp> unitComps = new Dictionary<int, UnitComp>();

    public Transform TargetOrderBeacon 
    { 
        get { return target; } 
        set 
        { 
            target = value;
            if (movementComp) { movementComp.TargetBeacon = value; }
        } 
    }

    private Transform target;

    public GameObject Object => gameObject;
    public IOrderable OrderableComp => this;

    public GroupingSlotComp GroupingSlot => throw new System.NotImplementedException();

    public Transform RootTransform => transform;

    private UnitMovement movementComp;

    public void Setup(Dictionary<int,SelectableUnit> selection, Dictionary<int, UnitComp> comps, FormationSettings formation)
    {
        unitSelections = selection;
        unitComps = comps;

        foreach (var unit in unitSelections)
        {
            unit.Value.GroupingSlot.MyFormationGroup = this;
        }

        if (formation == null)
        { SetFormation(defaultFormation); }
        else 
        { SetFormation(formation); }

        //TODO replace this with a dynamic way of setting what movement to use.
        movementComp = gameObject.AddComponent<LightShipMovementComp>();
    }

    public void SetFormation(FormationSettings formation)
    {
        int sectionCount = 0;
        float currentXOffset = 0f;
        float currentYOffset = 0f;
        float currentZOffSet = formation.ZOffset;

        if (formation.XOffset != 0f)
        {
            sectionCount += 2;
            currentXOffset = formation.XOffset;
        }

        if (formation.YOffset != 0f)
        {
            sectionCount += 2;
            currentYOffset = formation.YOffset;
        }

        bool isFirstCase = formation.IsFirstIndexCenter;

        var currentPos = transform.position;

        int loopCount = 1;

        foreach (var unit in unitSelections.Values)
        {
            //var unitRoot = unit.transform.parent;
            var unitRoot = unit.OrderableComp;
            if(unitRoot == null) { continue; }

            if (isFirstCase)
            {
                isFirstCase = false;
                // set it to move to objects position
                var commandBeacon = Instantiate(orderBeaconPrefab, currentPos, Quaternion.identity);
                unitRoot.TargetOrderBeacon = commandBeacon;
                unitRoot.RootTransform.parent = transform;
                continue;
            }

            

            Vector3 placement = currentPos;
            switch (loopCount)
            {
                case 1:
                    placement += new Vector3(currentXOffset, currentYOffset, currentZOffSet);
                    break;

                case 2:
                    placement += new Vector3(-currentXOffset, currentYOffset, currentZOffSet);
                    break;

                case 3:
                    placement += new Vector3(currentXOffset, -currentYOffset, currentZOffSet);
                    break;

                case 4:
                    placement += new Vector3(-currentXOffset, -currentYOffset, currentZOffSet);
                    break;
            }
            //set target to be placement, spawn order beacon at pos to move unit there
            var beacon = Instantiate(orderBeaconPrefab, placement, Quaternion.identity);
            unitRoot.TargetOrderBeacon = beacon;
            unitRoot.RootTransform.parent = transform;
            loopCount++;

            if (loopCount > sectionCount)
            {
                currentXOffset += formation.XOffset;
                currentYOffset += formation.YOffset;
                currentZOffSet += formation.ZOffset;
                loopCount = 1;
            }
        }

    }

    public bool AddUnit(SelectableUnit entity)
    {
        return false;
    }

    public bool RemoveUnit(SelectableUnit entity)
    {
        return false;
    }

    public SelectableUnit GetCommanderUnit() { return null; }

    public void OnSelected()
    {
        foreach (var unit in unitSelections)
        {
            unit.Value.OnSelected();
        }
    }

    public void DeSelect()
    {
        foreach (var unit in unitSelections)
        {
            unit.Value.DeSelect();
        }
    }

    public ISelectable GetSelectable()
    {
        return this;
    }

}