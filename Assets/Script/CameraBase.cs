using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBase : MonoBehaviour
{
    public float ArmLength = 5f;

    private Transform CameraHolderRef;

    private void OnValidate()
    {
        Setup();
    }

    void Start()
    {
        Setup();
    }

    protected virtual void Setup()
    {
        if (CameraHolderRef) { return; }
        CameraHolderRef = transform.GetChild(0);
        CameraHolderRef.transform.localPosition = new Vector3(0f, 0f, -ArmLength); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawRay(CameraHolderRef.position, CameraHolderRef.forward*2f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, CameraHolderRef.position);
    }
}
