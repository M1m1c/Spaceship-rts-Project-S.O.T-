using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableEntity : MonoBehaviour
{
    private Color startcol;
    private Renderer myRenderer;
    void Start()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
        startcol = myRenderer.material.color;    
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
