using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShipMovementComp : UnitMovement
{

    public override Transform Target
    {
        get { return target; }
        set
        {
            target = value;
            if (reachedHorizontalTarget)//or the rotation is too large
            {
                travelVelocity = 0f;
            }
            reachedHorizontalTarget = false;
            reachedVerticalTarget = false;
            targetDistance = Vector3.Distance(transform.position, Target.transform.position);
            targetDirection = Target.transform.position - transform.position;
            var rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 1f);
            targetRotation = Quaternion.LookRotation(rotationDirection);

            rotationModifier = 1f;
            travelModifier = 1f;
            vertSpeedMod = 1f;
            horiSpeedMod = 1f;

            finalRotation = Quaternion.LookRotation(Target.transform.forward);
        }
    }
    private Transform target;


    private float maxAngularVelocity = 5f;

    private Rigidbody rb;

    private Quaternion targetRotation;
    private Quaternion finalRotation;
    private Vector3 targetDirection;
    private float targetDistance;
    private float travelVelocity = 0f;
    private float rotationVelocity = 0f;
    private float travelModifier = 1f;
    private float rotationModifier = 1f;
    private float distanceToTarget = 2f;
    private float angleToTarget = 2f;
    private bool reachedHorizontalTarget = false;
    private bool reachedVerticalTarget = false;
    private bool allowedToMoveH = true;

    //TODO set these using scriptabel object for each unit
    private float rotationSpeed = 0.1f;
    private float travelAngle = 15f;
    private float travelSpeed = 50f;
    private float travelDecelerationSpeed = 0.99f;
    private float travelAccelerationSpeed = 1f;

    private float vertSpeedMod = 1f;
    private float horiSpeedMod = 1f;

    //TODO these should probably be related to how fast we are moving ie how many units per frame,
    //so that even at faster travel speeds we are still able to hit the target and not overshoot it
    private const float HDistThreshold = 0.3f;
    private const float VDistThreshold = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    private void FixedUpdate()
    {
        MoveUnitToTarget();
    }

    private void UpdateTravelVelocity()
    {
        var currentDist = Vector3.Distance(transform.position, Target.transform.position);
        var decelDist = targetDistance * 0.2f;

        var velocityChange = VelocityCalc.GetVelocityChangeBasedOnDistFixed(
            travelVelocity,
            travelDecelerationSpeed,
            decelDist,
            travelAccelerationSpeed,
            decelDist <= currentDist);

        travelVelocity = Mathf.Clamp(travelVelocity + velocityChange, 0f, 1f);
    }

    private void UpdateRotationVelocity()
    {
        var velocityChange = VelocityCalc.GetVelocityChange(
            rotationVelocity,
            travelDecelerationSpeed,
            travelAccelerationSpeed,
            angleToTarget > 1f && distanceToTarget > 1f);

        rotationVelocity = Mathf.Clamp(rotationVelocity + velocityChange, 0f, 1f);
    }

    protected override void MoveUnitToTarget()
    {
        if (rb == null) { return; }
        if (Target == null) { return; }

        distanceToTarget = Vector3.Distance(transform.position, Target.transform.position);
        angleToTarget = Vector3.Angle(transform.forward, targetDirection);

        UpdateRotationVelocity();
        targetDirection = Target.transform.position - transform.position;

        if (distanceToTarget < 1f)
        {
            //TODO fix when vertical move is longer that it does not rotate correctly at the end of movement
            targetRotation = finalRotation;
            //TODO when done rotationg to final rotation remove target
        }
        else
        {
            var rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 1f);
            targetRotation = Quaternion.LookRotation(rotationDirection);
        }

        var rotVel = travelVelocity + (rotationVelocity);
        var rotSpeed = travelSpeed * (rotationSpeed * rotationModifier);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            Time.fixedDeltaTime * travelSpeed + (rotSpeed * rotVel));


        if (reachedVerticalTarget && reachedHorizontalTarget) { return; }

        //TODO might want to gradually transition between these modfier values,
        //because now it can be a bit snappy 
        if (distanceToTarget < 15f)
        {
            if (angleToTarget > travelAngle)
            {
                rotationModifier = 1.5f;
                travelModifier = 0.5f;
            }
            else
            {
                rotationModifier = 1f;
                travelModifier = 1f;
            }
        }

        UpdateTravelVelocity();

        var vPos = new Vector3(0f, transform.position.y, 0f);
        var vTargetPos = new Vector3(0f, Target.transform.position.y, 0f);
        var verticalDistance = Vector3.Distance(vPos, vTargetPos);

        var hPos = new Vector3(transform.position.x, 0f, transform.position.z);
        var hTargetPos = new Vector3(Target.transform.position.x, 0f, Target.transform.position.z);
        var horizontalDistance = Vector3.Distance(hPos, hTargetPos);

        if (horizontalDistance > verticalDistance)
        { vertSpeedMod = verticalDistance / horizontalDistance; }
        else
        { horiSpeedMod = horizontalDistance / verticalDistance; }

        if (!reachedVerticalTarget)
        {
            if (verticalDistance < VDistThreshold)
            {
                reachedVerticalTarget = true;
            }
            var verticalDir = new Vector3(0f, targetDirection.y, 0f).normalized;
            var vertSpeed = (travelSpeed * (travelVelocity * travelModifier) * vertSpeedMod) * Time.fixedDeltaTime;
            transform.position += verticalDir * vertSpeed;
        }

        if (!reachedHorizontalTarget)
        {
            if (horizontalDistance < HDistThreshold)
            {
                reachedHorizontalTarget = true;
            }
            var horizontalDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            var horiSpeed = (travelSpeed * (travelVelocity * travelModifier) * horiSpeedMod) * Time.fixedDeltaTime;
            transform.position += horizontalDir * horiSpeed;
        }

        if (reachedVerticalTarget && reachedHorizontalTarget) { transform.position = target.position; }
    }
}
