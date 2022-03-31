using UnityEngine;
using UnityEngine.Events;

public class SelectionBox3D : MonoBehaviour
{
    public UnityEvent<SelectableUnit> SelectionChanged = new UnityEvent<SelectableUnit>();
    private void OnTriggerEnter(Collider other)
    {
        if (!other) { return; }
        var entity= other.gameObject.GetComponent<SelectableUnit>();
        if (!entity) { return; }
        SelectionChanged.Invoke(entity);
    }
}
