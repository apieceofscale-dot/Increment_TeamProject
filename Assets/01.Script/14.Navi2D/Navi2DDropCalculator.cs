using UnityEngine;

public static class Navi2DDropCalculator
{
    public static bool TryCalculateDropVelocity(
        Vector2 startPosition,
        Vector2 TargetPosition,
        float moveSpeed,
        float gravity,
        out Vector2 dropVelocity)
    {
        return TryCalculateDropVelocity(startPosition, TargetPosition, moveSpeed,
            gravity, 0f, out dropVelocity, out _);

    }


    // Start at the platform edge, or use the actual downward speed after leaving it.
    public static bool TryCalculateDropVelocity(Vector2 startPosition,
        Vector2 targetPosition, float moveSpeed, float gravity, float initialVelocityY,
        out Vector2 dropVelocity, out float fallTime)
    {
        dropVelocity = Vector2.zero;
        fallTime = 0f;
        float height = startPosition.y - targetPosition.y;
        if (height <= 0.01f || gravity <= 0f || initialVelocityY > 0f)
            return false;

        // Stable positive root of h + vy*t - g*t*t/2 = 0.
        float root = Mathf.Sqrt(initialVelocityY * initialVelocityY + 2f * gravity * height);
        float time = 2f * height / (root - initialVelocityY);
        float vx = (targetPosition.x - startPosition.x) / time;
        if (float.IsNaN(vx) || float.IsInfinity(vx) || Mathf.Abs(vx) > Mathf.Abs(moveSpeed))
            return false;

        dropVelocity = new Vector2(vx, initialVelocityY);
        fallTime = time;
        return true;
    }
}
   
