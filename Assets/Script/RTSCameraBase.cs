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

    private Vector2 horizontalMoveDirection = new Vector2();
    private float verticalMoveDirection;
    private float minMoveSpeed = 15f;
    private float MaxmoveSpeed = 20f;

    private Vector2 rotationDirection = new Vector2();
    private float rotationSpeed = 30f;
    private bool invertVerticalRot = false;
    private bool inverthorizontalRot = false;
    private bool rotationToggle = false;

    private float zoomDirection = 0;
    private float maxZoomOut = 200f;
    private float zoomSpeed = 7f;
    private float currentZoom = 5f;

    public void InputHorizontalMoveDirection(InputAction.CallbackContext context)
    {
        horizontalMoveDirection = context.ReadValue<Vector2>();
    }

    public void InputVerticalMoveDirection(InputAction.CallbackContext context)
    {
        verticalMoveDirection = context.ReadValue<float>();
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
        //Cursor.lockState = CursorLockMode.Locked;
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
    }

    private void Update()
    {
        var currentMoveSpeed = minMoveSpeed * Mathf.Sqrt(currentZoom) * Time.deltaTime;
        var forwardMovement = transform.forward * horizontalMoveDirection.y * currentMoveSpeed;
        var sideMovement = transform.right * horizontalMoveDirection.x * currentMoveSpeed;
        var verticalMovement = transform.up * verticalMoveDirection * currentMoveSpeed;
        transform.Translate(forwardMovement + sideMovement + verticalMovement, Space.World);

        if (rotationToggle)
        {
            var rotSpeed = rotationSpeed * Time.deltaTime;
            var rotX = inverthorizontalRot ? -rotationDirection.x : rotationDirection.x;
            transform.Rotate(new Vector3(0f, rotX*rotSpeed, 0f));

            if (CameraPivotRef)
            {
                var rotY = invertVerticalRot ? rotationDirection.y : -rotationDirection.y;
                var verticalRot = Quaternion.Euler(CameraPivotRef.eulerAngles.x + (rotY * rotSpeed), 0f, 0f);
                CameraPivotRef.localRotation = verticalRot;
            }
        }

        if (CameraHolderRef)
        {
            currentZoom = Mathf.Clamp(Mathf.Abs(CameraHolderRef.localPosition.z) - zoomDirection * zoomSpeed * Time.deltaTime, 1f, maxZoomOut);
            var zoomPos = new Vector3(CameraHolderRef.localPosition.x, CameraHolderRef.localPosition.y, -currentZoom);
            CameraHolderRef.localPosition = zoomPos;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1f);
        Gizmos.DrawRay(CameraHolderRef.position, CameraHolderRef.forward * 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, CameraHolderRef.position);
    }
}
