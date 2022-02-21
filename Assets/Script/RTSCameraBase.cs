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
    private float maxMoveSpeed = 20f;
    private float moveVelocity = 0f;
    private float moveAccelerationSpeed = 2f;
    private float moveDecelerationSpeed = 3f;
    private bool moveAccOrDec = false;

    private Vector2 rotationDirection = new Vector2();
    private float rotationSpeed = 30f;
    private bool invertVerticalRot = false;
    private bool inverthorizontalRot = false;
    private bool rotationToggle = false;
    private float rotVelocity = 0f;
    private float rotAccelerationSpeed = 5f;
    private float rotDecelerationSpeed = 4.5f;
    private bool rotAccOrDec = false;


    private float zoomDirection = 0;
    private float maxZoomOut = 200f;
    private float zoomSpeed = 7f;
    private float currentZoom = 5f;

    public void InputHorizontalMoveDirection(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            moveAccOrDec = true;
            horizontalMoveDirection = context.ReadValue<Vector2>();
        }
        else if (context.canceled) { moveAccOrDec = false; }
    }

    public void InputVerticalMoveDirection(InputAction.CallbackContext context)
    {
        verticalMoveDirection = context.ReadValue<float>();
    }

    public void InputRotationDirection(InputAction.CallbackContext context)
    {

        if (context.performed && rotationToggle)
        {
            rotAccOrDec = true;
            rotationDirection = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        { rotAccOrDec = false; }
    }

    public void InputRotationToggle(InputAction.CallbackContext context)
    {
        if (context.performed) { rotationToggle = true; }
        else if (context.canceled)
        {
            rotationToggle = false;
            rotAccOrDec = false;
        }
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
        UpdateMoveVelocity();
        var currentHorizontalMoveSpeed = (minMoveSpeed * moveVelocity) * Mathf.Sqrt(currentZoom) * Time.deltaTime;
        //TODO implement velocity and acceleration for vertical movement too
        var currentVerticalMoveSpeed = minMoveSpeed * Mathf.Sqrt(currentZoom) * Time.deltaTime;
        var forwardMovement = transform.forward * horizontalMoveDirection.y * currentHorizontalMoveSpeed;
        var sideMovement = transform.right * horizontalMoveDirection.x * currentHorizontalMoveSpeed;
        var horizontalMovement = forwardMovement + sideMovement;
        var verticalMovement = transform.up * verticalMoveDirection * currentVerticalMoveSpeed;
        transform.Translate(horizontalMovement + verticalMovement, Space.World);


        UpdateRotVelocity();
        var rotSpeed = (rotationSpeed * rotVelocity) * Time.deltaTime;
        var rotX = inverthorizontalRot ? -rotationDirection.x : rotationDirection.x;
        transform.Rotate(new Vector3(0f, rotX * rotSpeed, 0f));

        if (CameraPivotRef)
        {
            var rotY = invertVerticalRot ? rotationDirection.y : -rotationDirection.y;
            var verticalRot = Quaternion.Euler(CameraPivotRef.eulerAngles.x + (rotY * rotSpeed), 0f, 0f);
            CameraPivotRef.localRotation = verticalRot;
        }


        //TODO implement velocity and acceleration for zoom too
        if (CameraHolderRef)
        {
            currentZoom = Mathf.Clamp(Mathf.Abs(CameraHolderRef.localPosition.z) - zoomDirection * zoomSpeed * Time.deltaTime, 1f, maxZoomOut);
            var zoomPos = new Vector3(CameraHolderRef.localPosition.x, CameraHolderRef.localPosition.y, -currentZoom);
            CameraHolderRef.localPosition = zoomPos;
        }
    }

    void UpdateMoveVelocity()
    {
        var velocityChange = GetVelocityChange(
            moveVelocity,
            moveDecelerationSpeed,
            moveAccelerationSpeed,
            moveAccOrDec);

        moveVelocity = Mathf.Clamp(moveVelocity + velocityChange, 0f, 1f);
    }

    void UpdateRotVelocity()
    {
        var velocityChange = GetVelocityChange(
            rotVelocity,
            rotDecelerationSpeed,
            rotAccelerationSpeed,
            rotationToggle && rotAccOrDec);

        rotVelocity = Mathf.Clamp(rotVelocity + velocityChange, 0f, 1f);
    }

    private float GetVelocityChange(float velocity,float decelerationSpeed,float accelerationSpeed,bool changeCondition)
    {
        var proportionalDec = -(Time.deltaTime + (Time.deltaTime * (decelerationSpeed * velocity)));
        var deceleration = velocity > 0f ? proportionalDec : -Time.deltaTime;
        var velocityChange = (changeCondition ? Time.deltaTime * accelerationSpeed : deceleration);
        return velocityChange;
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
