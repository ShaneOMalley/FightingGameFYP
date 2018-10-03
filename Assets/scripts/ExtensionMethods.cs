using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static bool IsIntersecting(this BoxCollider2D[] colliders, BoxCollider2D[] other)
    {
        foreach (BoxCollider2D other_collider in other)
        {
            foreach (BoxCollider2D collider in colliders)
            {
                if (collider.bounds.Intersects(other_collider.bounds))
                    return true;
            }
        }

        return false;
    }

    public static bool IsIntersecting(this BoxCollider2D collider, BoxCollider2D other)
    {
        return collider.bounds.Intersects(other.bounds);
    }
}
