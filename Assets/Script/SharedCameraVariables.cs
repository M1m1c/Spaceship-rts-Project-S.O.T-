using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedCameraVariables : MonoBehaviour
{
    public SelectionCollection selectionCollection { get; private set; }
    public GroupOrigin currentGroupOrigin { get; private set; }

    public GroupOrigin groupOriginPrefab;

    void Start()
    {
        selectionCollection = GetComponent<SelectionCollection>();
        if (groupOriginPrefab) { currentGroupOrigin = Instantiate(groupOriginPrefab); }
    }

}
