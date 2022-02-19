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
    private float moveSpeed = 0.3f;

    private Vector2 rotationDirection = new Vector2();
    private float rotationSpeed = 0.5f;
    private bool invertVerticalRot = false;
    private bool inverthorizontalRot = false;
    private bool rotationToggle = false;

    private float zoomDirection = 0;
    private float maxZoomOut = 200f;
    private float zoomSpeed = 7f;
    private float currentZoom = 5f;
    public void InputMoveDirection(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void InputRotationDirection(InputAction.CallbackContext context)
    {
        rotationDirection = context.ReadValue<Vector2>();
    }

    public void InputRotationToggle(InputAction.CallbackContext context)
    {
        if (context.performed) { rotationToggle = true; }
        else if (context.canceled) { rotationToggle = false; }
    }

    public void InputZoomDirection(InputAction.CallbackContext context)
    {
        zoomDirection = context.ReadValue<float>();
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
            currentZoom = defaultArmLength;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        var currentMoveSpeed = moveSpeed * Mathf.Sqrt(currentZoom);
        var forwardMovement = transform.forward * moveDirection.y * currentMoveSpeed;
        var sideMovement = transform.right * moveDirection.x * currentMoveSpeed;
        transform.Translate(forwardMovement + sideMovement, Space.World);

        if (rotationToggle)
        {
            var rotX = inverthorizontalRot ? -rotationDirection.x : rotationDirection.x;
            transform.Rotate(new Vector3(0f, rotX, 0f));

            if (CameraPivotRef)
            {
                var rotY = invertVerticalRot ? rotationDirection.y : -rotationDirection.y;
                var verticalRot = Quaternion.Euler(CameraPivotRef.eulerAngles.x + rotY * rotationSpeed, 0f, 0f);
                CameraPivotRef.localRotation = verticalRot;
            }
        }
    }

    private void Update()
    {
        if (CameraHolderRef)
        {
            currentZoom = Mathf.Clamp(Mathf.Abs(CameraHolderRef.localPosition.z) - zoomDirection*zoomSpeed * Time.deltaTime, 1f, maxZoomOut);
            var zoomPos = new Vector3(CameraHolderRef.localPosition.x, CameraHolderRef.localPosition.y, -currentZoom);
            CameraHolderRef.localPosition = zoomPos;
        }
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
