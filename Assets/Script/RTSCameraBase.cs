using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraBase : MonoBehaviour
{
    private float defaultArmLength = 5f;

    private Transform CameraPivotRef;
    private Transform CameraHolderRef;

    //TODO make settings json or scriptable object used to set some fo these settings

    private Vector2 moveDirection = new Vector2();
    private float moveSpeed = 0.5f;

    private Vector2 rotationDirection = new Vector2();
    private float rotationSpeed = 0.5f;
    private bool invertVerticalRot = false;
    private bool inverthorizontalRot = false;

    public void UpdateMoveDirection(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void UpdateRotationDirection(InputAction.CallbackContext context)
    {
        rotationDirection = context.ReadValue<Vector2>();
    }

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
        if (!CameraPivotRef)
        {
            CameraPivotRef = transform.GetChild(0);
        }

        if (!CameraHolderRef)
        {
            CameraHolderRef = transform.GetChild(0).GetChild(0);
            CameraHolderRef.transform.localPosition = new Vector3(0f, 0f, -defaultArmLength);
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var forwardMovement = transform.forward * moveDirection.y * moveSpeed;
        var sideMovement = transform.right * moveDirection.x * moveSpeed;
        transform.Translate(forwardMovement + sideMovement, Space.World);

        var rotX = inverthorizontalRot ? rotationDirection.x : -rotationDirection.x;
        transform.Rotate(new Vector3(0f, rotX, 0f));

        if (CameraPivotRef)
        {
            var rotY = invertVerticalRot ? rotationDirection.y : -rotationDirection.y;
            var verticalRot = Quaternion.Euler(CameraPivotRef.eulerAngles.x + rotY * rotationSpeed, 0f, 0f);
            CameraPivotRef.localRotation = verticalRot;
        }
        //transform.position = transform.position + new Vector3(moveDirection.x,0f,moveDirection.y);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawRay(CameraHolderRef.position, CameraHolderRef.forward * 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, CameraHolderRef.position);
    }
}
