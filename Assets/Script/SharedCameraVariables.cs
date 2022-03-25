using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedCameraVariables : MonoBehaviour
{
    public SelectionCollection selectionCollection { get; private set; }
    public SelectionGroupOrigin currentGroupOrigin { get; private set; }

    public SelectionGroupOrigin groupOriginPrefab;

    void Start()
    {
        selectionCollection = GetComponent<SelectionCollection>();
        if (groupOriginPrefab) { currentGroupOrigin = Instantiate(groupOriginPrefab); }
    }

}
