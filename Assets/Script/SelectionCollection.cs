using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCollection : MonoBehaviour
{

    public Dictionary<int, SelectableEntity> SelectedEnteties = new Dictionary<int, SelectableEntity>();

    public void AddSelectedEntity(SelectableEntity entity)
    {
        int id = entity.gameObject.GetInstanceID();
        if (SelectedEnteties.ContainsKey(id)) { return; }

        SelectedEnteties.Add(id, entity);
        entity.gameObject.AddComponent<SelectedComp>();
    }

    public int DeselectEntity(SelectableEntity entity, bool removeID)
    {

        int id = entity.gameObject.GetInstanceID();
        if (!SelectedEnteties.ContainsKey(id)) { return -1; }
        entity.GetComponent<SelectedComp>().DeSelect();

        if (removeID)
        {
            SelectedEnteties.Remove(id);
        }
        return id;
    }

    public void DeselectAllEntties()
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
