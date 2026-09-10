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
        dropVelocity = Vector2.zero;
        float deltaX = TargetPosition.x - startPosition.x;
        float deltaY = TargetPosition.y - startPosition.y;

        if (deltaY >= 0f) return false;
        if (Mathf.Abs(deltaX) < 0.01f) return false; // 뭔지 물어보고 넘어가기.
        float fallDistacne = Mathf.Abs(deltaY);

        float fallTime = Mathf.Sqrt(2f * fallDistacne / gravity);

        float requiredVelocityX = deltaX / fallTime;

        if (Mathf.Abs(requiredVelocityX) > Mathf.Abs(moveSpeed)) return false;

        dropVelocity = new Vector2(requiredVelocityX, 0f);

        return true;

    }


}
   
