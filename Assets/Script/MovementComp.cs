using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementComp : UnitMovement
{


    public Transform Target
    {
        get { return target; }
        set
        {
            target = value;
            if (reachedHorizontalTarget)//or the rotation is too large
            {
                travelVelocity = 0f;
            }
            rotationProgress = 0f;
            reachedHorizontalTarget = false;
            reachedVerticalTarget = false;
            targetDistance = Vector3.Distance(transform.position, Target.transform.position);
            targetDirection = Target.transform.position - transform.position;
            var rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 1f);
            targetRotation = Quaternion.LookRotation(rotationDirection);
        }
    }
    private Transform target;


    private float maxAngularVelocity = 5f;

    private Rigidbody rb;

    private Quaternion targetRotation;
    private Vector3 targetDirection;
    private float targetDistance;
    private float rotationProgress = 0f;
    private float travelVelocity = 0f;
    private bool reachedHorizontalTarget = false;
    private bool reachedVerticalTarget = false;

    //TODO set these using scriptabel object for each unit
    private float rotationSpeed = 0.1f;
    private float travelAngle = 0.5f;
    private float travelSpeed = 50f;
    private float travelDecelerationSpeed = 0.99f;
    private float travelAccelerationSpeed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    private void FixedUpdate()
    {
        if (rb == null) { return; }
        if (Target == null) { return; }

        ////Large ship movement
        //if (rotationProgress != 1f)
        //{
        //    rotationProgress += Time.fixedDeltaTime * rotationSpeed;
        //    rotationProgress = Mathf.Clamp01(rotationProgress);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationProgress);
        //}

        //if (!reachedTarget )
        //{
        //    if (Vector3.Distance(transform.position, Target.transform.position) < 1f)
        //    {
        //        rb.velocity = Vector3.zero;
        //        reachedTarget = true;
        //    }

        //    //horizontal Movement
        //    if (Vector3.Dot(transform.forward, targetDirection) >= travelAngle) 
        //    {        
        //        UpdateTravelVelocity();
        //        //transform.Translate((-targetDirection.normalized) * (travelSpeed * travelVelocity) * Time.fixedDeltaTime);
        //        rb.AddForce(targetDirection.normalized * (travelSpeed * travelVelocity) * Time.fixedDeltaTime);
        //        //rb.AddForce(targetDirection.normalized * (travelSpeed * travelVelocity) * Time.fixedDeltaTime);
        //        //rb.AddForce(transform.forward * (travelSpeed * travelVelocity) * Time.fixedDeltaTime);
        //    }
        //}

        //TODO rework rotational to be able to update dynamically better
        //Dont use lerp use math instead
        //Large ship movement     
        if (rotationProgress != 1f)
        {
            rotationProgress += Time.fixedDeltaTime * rotationSpeed;
            rotationProgress = Mathf.Clamp01(rotationProgress);
            //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationProgress);
        }

        targetDirection = Target.transform.position - transform.position;
        var rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 1f);
        targetRotation = Quaternion.LookRotation(rotationDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * travelSpeed+((travelSpeed*0.1f)*travelVelocity));
    


        //if (Vector3.Distance(transform.position, Target.transform.position) < 1f)
        //{
        //    rb.velocity = Vector3.zero;
        //}

        if (!reachedVerticalTarget)
        {
            var vPos = new Vector3(0f, transform.position.y, 0f);
            var vTargetPos = new Vector3(0f, Target.transform.position.y, 0f);
            var verticalDistance = Vector3.Distance(vPos, vTargetPos);

            if (verticalDistance < 1f)
            {
                reachedVerticalTarget = true;
            }

            var verticalDir = new Vector3(0f, targetDirection.y, 0f).normalized;
            transform.position += verticalDir * (travelSpeed * 0.3f) * Time.fixedDeltaTime;
        }

        if (!reachedHorizontalTarget)
        {
            var hPos = new Vector3(transform.position.x, 0f, transform.position.z);
            var hTargetPos = new Vector3(Target.transform.position.x, 0f, Target.transform.position.z);
            var horizontalDistance = Vector3.Distance(hPos, hTargetPos);

            if (horizontalDistance < 1f)
            {
                reachedHorizontalTarget = true;
            }
            UpdateTravelVelocity();
            var horizontalDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            transform.position += horizontalDir * (travelSpeed * travelVelocity) * Time.fixedDeltaTime;
            //if (Vector3.Dot(transform.forward, targetDirection) >= travelAngle)
            //{
            //    UpdateTravelVelocity();
            //    //small ship movement
            //    var horizontalDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; //new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;
            //    //Large ship movement
            //    //var horizontalDir = new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;
            //    transform.position += horizontalDir * (travelSpeed * travelVelocity) * Time.fixedDeltaTime;
            //}
        }



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

    protected override void MoveUnitToTarget()
    {
        if (rb == null) { return; }
        if (Target == null) { return; }
       
        targetDirection = Target.transform.position - transform.position;
        var rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 1f);
        targetRotation = Quaternion.LookRotation(rotationDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * travelSpeed + ((travelSpeed * 0.1f) * travelVelocity));


        if (!reachedVerticalTarget)
        {
            var vPos = new Vector3(0f, transform.position.y, 0f);
            var vTargetPos = new Vector3(0f, Target.transform.position.y, 0f);
            var verticalDistance = Vector3.Distance(vPos, vTargetPos);

            if (verticalDistance < 1f)
            {
                reachedVerticalTarget = true;
            }

            var verticalDir = new Vector3(0f, targetDirection.y, 0f).normalized;
            transform.position += verticalDir * (travelSpeed * 0.3f) * Time.fixedDeltaTime;
        }

        if (!reachedHorizontalTarget)
        {
            var hPos = new Vector3(transform.position.x, 0f, transform.position.z);
            var hTargetPos = new Vector3(Target.transform.position.x, 0f, Target.transform.position.z);
            var horizontalDistance = Vector3.Distance(hPos, hTargetPos);

            if (horizontalDistance < 1f)
            {
                reachedHorizontalTarget = true;
            }
            UpdateTravelVelocity();
            var horizontalDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            transform.position += horizontalDir * (travelSpeed * travelVelocity) * Time.fixedDeltaTime;
        }
    }

    //TODO
    //Rotate based on a rotation speed to be aligned with travel path.
    //turn speed should determine how big the turns we have to make are.
    //Slowly accelerate until and reach full velocity when we are aligned with travel path.
    //Use angles to measure when we are aligned.
    // don't use physics for movement, only if we get rammed do we care about physics.
    //when calculating a path draw a straight line from our current position to next beacon,
    //if line gets blocked try checking in all 8 directions perpandicular form block point
    // until we find a place to place an inbetween beacon.
    // use a collsiion sphere around unit to detect if we are about to collide
    // with somethign around uss and nudge unit in opposite direction.
    // trace out infron of unit based on rotation speed, to check if path needs to be updated with a new beacon,
    // to acount for turns.
}
