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

        var spawnPos = selectionController.CurrentGroupOrigin.transform.position;
        //TODO check if there already is a formation group, so we dont need to spawn one
        //TODO take into account max units in formation,
        //and spawn multiple formation groups if a larger amount is selected.
        var group = Instantiate(FormationGroupPrefab, spawnPos, Quaternion.identity);

        group.Setup(selection, FormationSetting);
    }
}
