using UnityEngine;

public static class Navi2DJumpCalculator
{
    public static bool TryCalculateJumpVelocity(
        Vector2 startPosition,
        Vector2 targetPosition,
        float moveSpeed,
        float jumpMaxHeight,
        float gravity,
        out Vector2 jumpVelocity)

    {

        jumpVelocity = Vector2.zero;

        float horizontalSpeed = Mathf.Abs(moveSpeed);

        if (horizontalSpeed <= 0f) return false;

        float deltaX = targetPosition.x - startPosition.x;
        float deltaY = targetPosition.y - startPosition.y;

        float distanceX = Mathf.Abs(deltaX);

        if(distanceX <= 0.01f) return false;

        float time = distanceX / horizontalSpeed;

        float requiredVelocityY = (deltaY + 0.5f * gravity * time * time) / time;

        float maxVelocityY = Mathf.Sqrt(2f * gravity * jumpMaxHeight);

        if(requiredVelocityY > maxVelocityY) return false;

        float direction = Mathf.Sign(deltaX);

        jumpVelocity = new Vector2(direction *horizontalSpeed, requiredVelocityY);

        return true;

        
    }





}
    

