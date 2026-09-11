using UnityEngine;

public static class Navi2DAirMoveCalculator
{
    public static bool TryCalculateAirVelocity(
        Vector2 startPosition,
        Vector2 targetPosition,
        float airMoveSpeed,
        float jumpMaxHeight,
        float gravity,
        float clearanceMargin,
        float horizontalClearance,
        float obstacleTopY,
        float obstacleMinX,
        float obstacleMaxX,
        float ceilingBottomY,
        float ceilingMinX,
        float ceilingMaxX,
        float airBodyHeight,
        out Vector2 airVelocity)
    {              
        airVelocity = Vector2.zero;

        float maxHorizontalSpeed = Mathf.Abs(airMoveSpeed);
        
        if (maxHorizontalSpeed <= 0f) return false;
        if (gravity <= 0f) return false; 


        float deltaX = targetPosition.x - startPosition.x;
        float deltaY = targetPosition.y - startPosition.y;

        float distanceX = Mathf.Abs(deltaX);

        if(distanceX <= 0.01f) return false;

        float fastestTime = distanceX / maxHorizontalSpeed;

        float requiredVelocityY = (deltaY + 0.5f * gravity * fastestTime * fastestTime) / fastestTime;

        bool hasObstacle = !float.IsInfinity(obstacleMinX) && !float.IsInfinity(obstacleMaxX) && obstacleMinX <= obstacleMaxX;

        float requiredRise = 0f;
        if(requiredVelocityY > 0f)
        {
            requiredRise = requiredVelocityY * requiredVelocityY / (2f * gravity);
        }

        float obstacleRise = 0f;

        if (hasObstacle)
        {
            float obstacleRequiredPeakY = obstacleTopY + clearanceMargin;
            obstacleRise = obstacleRequiredPeakY - startPosition.y;
        }


        
        

        float targetRise = Mathf.Max(0f, deltaY);
        float needRise = Mathf.Max(requiredRise, targetRise);
        float selectedRise = Mathf.Max(needRise, obstacleRise);

        if (hasObstacle)
        {
            selectedRise += 0.05f;
        }

        if (selectedRise > jumpMaxHeight)
        {
           
            return false;
        }


        float velocityY = Mathf.Sqrt(2f * gravity * selectedRise);

        float discrimiant = velocityY * velocityY - 2f * gravity * deltaY;

        if(discrimiant <= 0f) return false;

        float flightTime = (velocityY + Mathf.Sqrt(discrimiant)) / gravity;

        if(flightTime <= 0f) return false;

        float velocityX = deltaX / flightTime;

        if (Mathf.Abs(velocityX) > maxHorizontalSpeed) return false;

        bool isSameHeight = Mathf.Abs(deltaY) < 0.1f;

        if (hasObstacle && isSameHeight)
        {
            float clearanceMinX = obstacleMinX - horizontalClearance;

            float clearanceMaxX = obstacleMaxX + horizontalClearance;

            if (deltaX > 0f)
            {
                if (startPosition.x >= clearanceMinX) return false;

                if (targetPosition.x <= clearanceMaxX) return false;
            }
            else
            {
                if (startPosition.x <= clearanceMaxX) return false;

                if (targetPosition.x >= clearanceMinX) return false;
            }
        }






        if (hasObstacle)
        {
            float clearanceMinX = obstacleMinX - horizontalClearance;
            float clearanceMaxX = obstacleMaxX + horizontalClearance;
            if (deltaX > 0f)
            {
                // 오른쪽으로 이동
                if (startPosition.x >= clearanceMinX)
                    return false;

                if (targetPosition.x <= clearanceMaxX)
                    return false;
            }
            else
            {
                // 왼쪽으로 이동
                if (startPosition.x <= clearanceMaxX)
                    return false;

                if (targetPosition.x >= clearanceMinX)
                    return false;
            }

            float requriedY = obstacleTopY + clearanceMargin;
            float timeAtMinX = (clearanceMinX - startPosition.x) / velocityX;
            float timeAtMaxX = (clearanceMaxX - startPosition.x) / velocityX;

            if (timeAtMinX >=0f && timeAtMinX <=flightTime)
            {
                float yAtminX = startPosition.y + velocityY *timeAtMinX - 0.5f *gravity * timeAtMinX *timeAtMinX;
                if (yAtminX < requriedY)
                {
                    return false;
                }
            }

            if (timeAtMaxX >= 0f && timeAtMaxX <= flightTime)
            {
                float yAtMaxX = startPosition.y + velocityY * timeAtMaxX - 0.5f * gravity * timeAtMaxX * timeAtMaxX;
                if (yAtMaxX < requriedY)
                {
                     return false;
                }
            }


        }



        bool hasCeiling = !float.IsInfinity(ceilingBottomY);

        if (hasCeiling)
        {
            float ceilingStartX = ceilingMinX - horizontalClearance;

            float ceilingEndX = ceilingMaxX + horizontalClearance;

            float time1 = (ceilingStartX - startPosition.x) / velocityX;

            float time2 = (ceilingEndX - startPosition.x) / velocityX;

            float enterTime = Mathf.Min(time1, time2);
            float exitTime = Mathf.Max(time1, time2);

            enterTime = Mathf.Max(0f, enterTime);
            exitTime = Mathf.Min(flightTime, exitTime);

            if (enterTime <= exitTime)
            {
                float apexTime = velocityY / gravity;

                float checkTime =
                    Mathf.Clamp(apexTime, enterTime, exitTime);

                float footY =
                    startPosition.y
                    + velocityY * checkTime
                    - 0.5f * gravity * checkTime * checkTime;

                float headY =
                    footY + airBodyHeight + 0.05f;

                if (headY > ceilingBottomY)
                {
                    Debug.Log($"Air 실패 : Ceiling / head={headY}, ceiling={ceilingBottomY}");

                    return false;
                }
            }
        }
        airVelocity = new Vector2(velocityX, velocityY);

        return true;
    }


    public static bool TryCalculateJumpUpVelocity(
        Vector2 startPosition,
        Vector2 targetPosition,
        float airMoveSpeed,
        float jumpMaxHeight,
        float gravity,
        float jumpClearance,
        float horizontalClearance,
        System.Func<Vector2, float, bool> isArcClear,
        out Vector2 jumpVelocity)
    {
        jumpVelocity = Vector2.zero;
        float deltaX = targetPosition.x - startPosition.x;
        float deltaY = targetPosition.y - startPosition.y;
        float maxSpeed = Mathf.Abs(airMoveSpeed);
        if (deltaY <= 0.1f || gravity <= 0f || maxSpeed <= 0f || isArcClear == null)
            return false;

        float minimumRise = deltaY + Mathf.Max(0.05f, jumpClearance);
        if (minimumRise > jumpMaxHeight) return false;

        // Try higher arcs too: the lowest arc can reach the target but hit its wall.
        const int samples = 64;
        for (int i = 0; i <= samples; i++)
        {
            float rise = Mathf.Lerp(minimumRise, jumpMaxHeight, (float)i / samples);
            float vy = Mathf.Sqrt(2f * gravity * rise);
            float time = (vy + Mathf.Sqrt(Mathf.Max(0f, vy * vy - 2f * gravity * deltaY))) / gravity;
            Vector2 velocity = new Vector2(deltaX / time, vy);
            if (Mathf.Abs(velocity.x) > maxSpeed) continue;
            if (!isArcClear(velocity, time)) continue;

            jumpVelocity = velocity;
            return true;
        }
        return false;
    }
}