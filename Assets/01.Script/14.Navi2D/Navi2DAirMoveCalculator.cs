using UnityEngine;

public static class Navi2DAirMoveCalculator
{
    public static bool TryCalculateAirVelocity(
        Vector2 startPosition,
        Vector2 targetPosition,
        float moveSpeed,
        float jumpMaxHeight,
        float gravity,
        float clearanceMargin,
        float obstacleTopY,
        out Vector2 airVelocity)
    {
        airVelocity = Vector2.zero;

        float maxHorizontalSpeed = Mathf.Abs(moveSpeed);

        if (maxHorizontalSpeed <= 0f) return false;
        if (gravity <= 0f) return false; 


        float deltaX = targetPosition.x - startPosition.x;
        float deltaY = targetPosition.y - startPosition.y;

        float distanceX = Mathf.Abs(deltaX);

        if(distanceX <= 0.01f) return false;

        float fastestTime = distanceX / maxHorizontalSpeed;

        float requiredVelocityY = (deltaY + 0.5f * gravity * fastestTime * fastestTime) / fastestTime; 

        float requiredRise = 0f;
        if(requiredVelocityY > 0f) 
        {
            requiredRise = requiredVelocityY * requiredVelocityY / (2f * gravity);
        }
        float obstacleRequiredPeakY = obstacleTopY + clearanceMargin;
        float obstacleRise = obstacleRequiredPeakY - startPosition.y;

        float targetRise = Mathf.Max(0f, deltaY);
        float needRise = Mathf.Max(requiredRise, targetRise);
        float selectedRise = Mathf.Max(needRise, obstacleRise); 

        if(selectedRise > jumpMaxHeight) return false;

        float velocityY = Mathf.Sqrt(2f * gravity * selectedRise);

        float discrimiant = velocityY * velocityY - 2f * gravity * deltaY;

        if(discrimiant < 0f) return false;

        float flightTime = (velocityY + Mathf.Sqrt(discrimiant)) / gravity;

        if(flightTime < 0f) return false;

        float velocityX = deltaX / flightTime;

        if(Mathf.Abs(velocityX) > maxHorizontalSpeed) return false;

        airVelocity = new Vector2(velocityX, velocityY);

        return true;        
    }





}
    

