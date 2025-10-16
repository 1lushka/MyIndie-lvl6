using UnityEngine;

public static class ObjectMover
{
    public static void MoveTo(Transform obj, Vector3 targetPosition)
    {
        if (obj == null) return;
        obj.position = targetPosition;
    }
}