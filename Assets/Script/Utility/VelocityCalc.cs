using UnityEngine;

public static class VelocityCalc 
{
    public static float GetVelocityChange(float velocity, float decelerationSpeed, float accelerationSpeed, bool changeCondition)
    {
        var proportionalDec = -(Time.deltaTime + (Time.deltaTime * (decelerationSpeed * velocity)));
        var deceleration = velocity > 0f ? proportionalDec : -Time.deltaTime;
        var velocityChange = (changeCondition ? Time.deltaTime * accelerationSpeed : deceleration);
        return velocityChange;
    }
}
