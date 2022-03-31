using System.Collections.Generic;
using UnityEngine;

//used for similar space ships to form formations and travel together
public class FormationGroup : MonoBehaviour
{
    public FormationSettings defaultFormation;
    public Transform orderBeaconPrefab;
    private FormationSettings currentFormation;
    private Dictionary<int, SelectableEntity> units = new Dictionary<int, SelectableEntity>();
    public void Setup(Dictionary<int,SelectableEntity> selection, FormationSettings formation)
    {
        units = selection;

        if (formation == null)
        { SetFormation(defaultFormation); }
        else 
        { SetFormation(formation); }
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

        foreach (var unit in units.Values)
        {
            //var unitRoot = unit.transform.parent;
            var unitRoot = unit.OrderableRoot;
            if(unitRoot == null) { continue; }

            if (isFirstCase)
            {
                isFirstCase = false;
                // set it to move to objects position
                var commandBeacon = Instantiate(orderBeaconPrefab, currentPos, Quaternion.identity);
                unitRoot.TargetOrderBeacon = commandBeacon;
                //unitRoot = transform;
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
            //unitRoot.transform.parent = transform;
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

    public bool AddUnit(SelectableEntity entity)
    {
        return false;
    }

    public bool RemoveUnit(SelectableEntity entity)
    {
        return false;
    }

    public SelectableEntity GetCommanderUnit() { return null; }


}