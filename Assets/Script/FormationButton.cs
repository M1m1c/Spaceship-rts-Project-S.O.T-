using System.Collections.Generic;
using UnityEngine;
public class FormationButton : UIButton
{
    public FormationSettings FormationSetting;
    public FormationGroup FormationGroupPrefab;
    public override void OnClick(SelectionController selectionController)
    {
        if (selectionController == null) { return; }

        var selection = selectionController.CurrentGroupOrigin.SelectionGroup;
        if (selection == null || (selection != null && selection.Count < 1)) { return; }

        Dictionary<int, SelectableUnit> unitSelections = new Dictionary<int, SelectableUnit>();
        List<int> selectionsToRemove = new List<int>();
        foreach (var item in selection)
        {
            var unitcomp = item.Value.OrderableComp as UnitComp;
            if (unitcomp == null)
            {
                selectionsToRemove.Add(item.Key);
                //selection.Remove(item.Key);
                continue;
            }

            var value = (SelectableUnit)item.Value;
            if (value)
            {
                unitSelections.Add(item.Key, value);
            }
        }

        for (int i = 0; i < selectionsToRemove.Count; i++)
        {
            selection.Remove(selectionsToRemove[i]);
        }

        var spawnPos = selectionController.CurrentGroupOrigin.transform.position;
        //TODO check if there already is a formation group, so we dont need to spawn one
        //TODO take into account max units in formation,
        //and spawn multiple formation groups if a larger amount is selected.
        var group = Instantiate(FormationGroupPrefab, spawnPos, Quaternion.identity);

        group.Setup(unitSelections, FormationSetting);

        var collection = selectionController.GetSelectionCollection;
        collection.DeselectAllEntities();
        collection.AddSelectedEntity(group);
    }
}
