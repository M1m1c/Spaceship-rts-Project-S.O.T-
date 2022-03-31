using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableEntity : MonoBehaviour, ISelectable
{
    private Color startcol;
    private Renderer myRenderer;

    public IOrderable OrderableRoot { get; private set; }

    void Start()
    {
        OrderableRoot = GetComponentInParent<IOrderable>();

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
}
