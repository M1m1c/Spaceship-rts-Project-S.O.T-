using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//used for similar space ships to form formations and travel together
public class FormationGroup : MonoBehaviour, ISelectable, IOrderable
{
    private UnityEvent<bool> commanderHasArrived = new UnityEvent<bool>();

    public FormationSettings defaultFormation;
    public Transform orderBeaconPrefab;
    private FormationSettings currentFormation;
    private Dictionary<int, SelectableUnit> unitSelections = new Dictionary<int, SelectableUnit>();
    //private Dictionary<int, UnitComp> unitComps = new Dictionary<int, UnitComp>();
    private IOrderable commanderUnit;

    public Transform TargetOrderBeacon
    {
        get { return target; }
        set
        {
            target = value;
            //if (commanderUnit != null) { commanderUnit.TargetOrderBeacon = value; }
            MoveFormationGroup(value, commanderUnit);
        }
    }

    private Transform target;

    public GameObject Object => gameObject;
    public IOrderable OrderableComp => this;

    public GroupingSlotComp GroupingSlot => throw new System.NotImplementedException();

    public Transform RootTransform => transform;

    //private UnitMovement movementComp;

    public void Setup(Dictionary<int, SelectableUnit> selection, FormationSettings formation)
    {
        unitSelections = selection;
        //unitComps = comps;

        foreach (var unit in unitSelections)
        {
            unit.Value.GroupingSlot.MyFormationGroup = this;
            var comp = (unit.Value.OrderableComp as UnitComp);
            if (!comp) { continue; }

            if (commanderUnit == null)
            {
                var unitRoot = unit.Value.OrderableComp;
                if (unitRoot != null)
                {
                    commanderUnit = unitRoot;
                    transform.parent = unit.Value.transform;
                    transform.localPosition = Vector3.zero;
                    comp.MovementComp.HasArrived.AddListener(commanderHasArrived.Invoke);
                    continue;
                }
            }
            
            
            commanderHasArrived.AddListener(comp.MovementComp.SetAllowedToArrive);
        }

        if (formation == null)
        { SetFormation(defaultFormation); }
        else
        { SetFormation(formation); }

    }

    public void SetFormation(FormationSettings formation)
    {
        Vector3 gatheringPoint = Vector3.zero;
        gatheringPoint += GroupPlaneCalc<SelectableUnit>.GetGroupPlane(unitSelections);
        var y = GroupPlaneCalc<SelectableUnit>.GetAverageYPos(unitSelections);
        gatheringPoint += new Vector3(0f, y, 0f);

        currentFormation = formation;
        var beacon = Instantiate(orderBeaconPrefab, gatheringPoint, Quaternion.identity);
        MoveFormationGroup(beacon,null);

    }

    private void MoveFormationGroup(Transform target, IOrderable commander)
    {
        int sectionCount = 0;
        float currentXOffset = 0f;
        float currentYOffset = 0f;
        float currentZOffSet = currentFormation.ZOffset;

        if (currentFormation.XOffset != 0f)
        {
            sectionCount += 2;
            currentXOffset = currentFormation.XOffset;
        }

        if (currentFormation.YOffset != 0f)
        {
            sectionCount += 2;
            currentYOffset = currentFormation.YOffset;
        }

        bool isFirstCase = currentFormation.IsFirstIndexCenter;

        int loopCount = 1;
        var comTransform = commander != null ? commanderUnit.RootTransform : target;
        Matrix4x4 targetMatrix = Matrix4x4.TRS(comTransform.position, comTransform.rotation, comTransform.localScale);
        foreach (var unit in unitSelections.Values)
        {
            var unitRoot = unit.OrderableComp;
            if (unitRoot == null) { continue; }

            if (isFirstCase)
            {
                isFirstCase = false;                
                unitRoot.TargetOrderBeacon = target;
                continue;
            }



            Vector3 placement = Vector3.zero;
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
            var beacon = Instantiate(orderBeaconPrefab, targetMatrix.MultiplyPoint3x4(placement), target.rotation, comTransform);
            unitRoot.TargetOrderBeacon = beacon;
            loopCount++;

            if (loopCount > sectionCount)
            {
                currentXOffset += currentFormation.XOffset;
                currentYOffset += currentFormation.YOffset;
                currentZOffSet += currentFormation.ZOffset;
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