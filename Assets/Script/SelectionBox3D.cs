using UnityEngine;
using UnityEngine.Events;

public class SelectionBox3D : MonoBehaviour
{
    public UnityEvent<SelectableEntity> SelectionChanged = new UnityEvent<SelectableEntity>();
    private void OnTriggerEnter(Collider other)
    {
        if (!other) { return; }
        var entity= other.gameObject.GetComponent<SelectableEntity>();
        if (!entity) { return; }
        SelectionChanged.Invoke(entity);
    }
}
