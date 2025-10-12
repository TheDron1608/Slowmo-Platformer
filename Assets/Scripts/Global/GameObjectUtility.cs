using UnityEngine;
using System;
using UnityEditor;

public static class GameObjectUtility
{
    public static void CopySpriteRenderer(SpriteRenderer from, SpriteRenderer to)
    {
        to.sprite = from.sprite;
        to.sharedMaterial = from.sharedMaterial;
        to.color = from.color;
        to.sortingOrder = from.sortingOrder;
    }

    public static void ConvertSimpleColliderToBoxCollider(BoxCollider2D result, Collider2D simpleCollider)
    {
        result.excludeLayers = simpleCollider.excludeLayers;
        result.includeLayers = simpleCollider.includeLayers;
        result.isTrigger = simpleCollider.isTrigger;

        if (simpleCollider.TryGetComponent(out BoxCollider2D boxCollider))
        {
            result.offset = boxCollider.offset;
            result.size = boxCollider.size;
        }
        else if (simpleCollider.TryGetComponent(out CircleCollider2D circleCollider))
        {
            result.offset = circleCollider.offset;
            result.size = Vector2.one * circleCollider.radius * 2;
        }
        else if (simpleCollider.TryGetComponent(out CapsuleCollider2D capsuleCollider))
        {
            result.offset = capsuleCollider.offset;
            result.size = capsuleCollider.size;
        }
        else
        {
            throw new UnityException("ConvertSimpleColliderToBoxCollider can apply only simple collider2Ds as parameter, " + simpleCollider.GetType() + " received instaed");
        }
    }

    public static Vector2 GetCenterOfCollider(Collider2D collider)
    {
        return collider.bounds.center;
    }

    public static bool TryGetComponentInSelfOrChild<T>(GameObject gameObject, out T component)
    {
        if (gameObject.TryGetComponent(out component)) return true;
        component = gameObject.GetComponentInChildren<T>();
        return component != null;
    }

    public static bool TryGetComponentInSelfOrParent<T>(GameObject gameObject, out T component)
    {
        return gameObject.TryGetComponent(out component) || gameObject.transform.parent.TryGetComponent(out component);
    }

    public static bool TryGetComponentInSelfOrParentOrChild<T>(GameObject gameObject, out T component)
    {
        if (TryGetComponentInSelfOrParent(gameObject, out component)) return true;
        component = gameObject.GetComponentInChildren<T>();
        return component != null;
    }

    public static T GetComponentWithPossibleFail<T>(GameObject getWhere) where T : Component
    {
        if (getWhere.TryGetComponent(out T component)) return component;
        return default;
    }
}
