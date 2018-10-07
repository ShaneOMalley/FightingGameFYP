using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static bool IsIntersecting(this BoxCollider2D[] colliders, BoxCollider2D[] other, out Vector3 hitPos)
    {
        foreach (BoxCollider2D other_collider in other)
        {
            foreach (BoxCollider2D collider in colliders)
            {
                if (collider.bounds.Intersects(other_collider.bounds))
                {
                    hitPos = Vector3.Lerp(collider.bounds.center, other_collider.bounds.center, 0.5f);
                    return true;
                }
            }
        }

        hitPos = Vector3.zero;
        return false;
    }

    public static bool IsIntersecting(this BoxCollider2D collider, BoxCollider2D other)
    {
        return collider.bounds.Intersects(other.bounds);
    }

    public static float NormalDeltaTime(this Time time)
    {
        return Time.deltaTime * 60;
    }
}
