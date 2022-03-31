using System.Collections.Generic;
using UnityEngine;

public class SelectionCollection : MonoBehaviour
{

    public Dictionary<int, ISelectable> SelectedEnteties = new Dictionary<int, ISelectable>();

    public void AddSelectedEntity(ISelectable entity)
    {
        entity = entity.GetSelectable();
        int id = entity.Object.GetInstanceID();

        if (SelectedEnteties.ContainsKey(id)) { return; }

        SelectedEnteties.Add(id, entity);
        entity.OnSelected();
    }

    public int DeselectEntity(ISelectable entity, bool removeID)
    {

        int id = entity.Object.GetInstanceID();
        if (!SelectedEnteties.ContainsKey(id)) { return -1; }
        entity.DeSelect();

        if (removeID)
        {
            SelectedEnteties.Remove(id);
        }
        return id;
    }

    public void DeselectAllEntities()
    {
        List<int> ids = new List<int>();
        foreach (var entity in SelectedEnteties)
        {
            ids.Add(DeselectEntity(entity.Value, false));
        }

        foreach (var id in ids)
        {
            if (!SelectedEnteties.ContainsKey(id)) { continue; }
            SelectedEnteties.Remove(id);
        }
    }
}
