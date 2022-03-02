using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupOrigin : MonoBehaviour
{
    private Transform myOrderBeacon;

    public Transform MyOrderBeacon
    {
        get { return myOrderBeacon; }
        set
        {
            myOrderBeacon = value;
            lineRenderer.enabled = value != null ? true : false;
        }
    }

    private LineRenderer lineRenderer;

    void Start()
    {

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 4;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (MyOrderBeacon)
        {
            var start = transform.position;
            var end = MyOrderBeacon.position;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.SetPosition(2, new Vector3(end.x, transform.position.y, end.z));
            lineRenderer.SetPosition(3, start);
        }
    }
}
