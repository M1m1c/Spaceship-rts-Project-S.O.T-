using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionGroupOrigin : MonoBehaviour, IOrderable
{
    private Transform myOrderBeacon;
    public Transform TargetOrderBeacon
    {
        get { return myOrderBeacon; }
        set
        {
            myOrderBeacon = value;
            lineRenderer.enabled = value != null ? true : false;
        }
    }


    public Dictionary<int, ISelectable> SelectionGroup { get; set; }

    public Transform RootTransform => throw new System.NotImplementedException();

    private LineRenderer lineRenderer;

    void Start()
    {

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 4;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        transform.position = GroupPlaneCalc<ISelectable>.GetGroupPlane(SelectionGroup);
        if (TargetOrderBeacon)
        {
            var start = transform.position;
            var end = TargetOrderBeacon.position;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.SetPosition(2, new Vector3(end.x, transform.position.y, end.z));
            lineRenderer.SetPosition(3, start);
        }
    }
}
