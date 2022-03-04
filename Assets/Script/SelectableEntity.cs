using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableEntity : MonoBehaviour
{

    private Transform myOrderBeacon;

    public Transform MyOrderBeacon
    {
        get { return myOrderBeacon; }
        set
        {
            myOrderBeacon = value;

            if (movementComp)
            {
                movementComp.Target = myOrderBeacon;
            }
        }
    }

    public Material LineMaterial;

    private Color startcol;
    private Renderer myRenderer;

    private MovementComp movementComp;

    void Start()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
        startcol = myRenderer.material.color;
        movementComp = GetComponentInParent<MovementComp>();
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
