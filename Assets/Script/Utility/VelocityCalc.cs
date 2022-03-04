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
    public static float GetVelocityChangeFixed(float velocity, float decelerationSpeed, float accelerationSpeed, bool changeCondition)
    {
        var proportionalDec = -(Time.fixedDeltaTime + (Time.fixedDeltaTime * (decelerationSpeed * velocity)));
        var deceleration = velocity > 0f ? proportionalDec : -Time.fixedDeltaTime;
        var velocityChange = (changeCondition ? Time.fixedDeltaTime * accelerationSpeed : deceleration);
        return velocityChange;
    }
    public static float GetVelocityChangeBasedOnDistFixed(float velocity, float decelerationSpeed, float decelDist, float accelerationSpeed, bool changeCondition)
    {
        var proportionalDec = -((velocity * velocity) / (decelerationSpeed * decelDist - velocity * (Time.fixedDeltaTime)));
        var deceleration = velocity > 0f ? proportionalDec : -Time.fixedDeltaTime;
        var velocityChange = (changeCondition ? Time.fixedDeltaTime * accelerationSpeed : deceleration);
        return velocityChange;
    }
}
