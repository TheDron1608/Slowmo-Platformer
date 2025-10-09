using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterStuckedObjects : AbstractCharacterComponent, IStuckToObject
{
    public float RemoveObjectVelocity = 3.5f;
    public float RemoveObjectMaxRandomAngularVelocity = 360f;

    private List<Holdable> _stuckedObjects = new();

    public List<Holdable> StuckedObjects
    {
        get => _stuckedObjects;
    }

    public void AddStuckedObject(Holdable obj)
    {
        _stuckedObjects.Add(obj);
        StartCoroutine(StuckObjectTrack(obj));
    }

    public void RemoveStuckedObject(Holdable obj)
    {
        _stuckedObjects.Remove(obj);
    }

    public void RemoveAllStuckedObjects()
    {
        for (int i = 0; i < _stuckedObjects.Count; i++)
        {
            Holdable stuckObject = _stuckedObjects[i];
            if (stuckObject == null) continue;

            stuckObject.StuckedToCollider = null;
            if (stuckObject.TryGetComponent(out Rigidbody2D stuckObjectRigidBody))
            {
                stuckObjectRigidBody.linearVelocity = VectorMath.GetAngleToAsNormalizedVec2(CharComponents.Center.transform.position, stuckObject.transform.position) * RemoveObjectVelocity * stuckObject.ThrowForceMultiplier;
                stuckObjectRigidBody.angularVelocity = RemoveObjectMaxRandomAngularVelocity * (UnityEngine.Random.value * 2 - 1);
            }
        }
    }

    public void RemoveAllStuckedObjects(Vector2 direction)
    {
        for (int i = 0; i < _stuckedObjects.Count; i++)
        {
            Holdable stuckObject = _stuckedObjects[i];
            if (stuckObject == null) continue;

            stuckObject.StuckedToCollider = null;
            if (stuckObject.TryGetComponent(out Rigidbody2D stuckObjectRigidBody))
            {
                stuckObjectRigidBody.linearVelocity = (VectorMath.GetAngleToAsNormalizedVec2(CharComponents.Center.transform.position, stuckObject.transform.position) + direction * 3).normalized * RemoveObjectVelocity * stuckObject.ThrowForceMultiplier;
            }
        }
    }

    private IEnumerator StuckObjectTrack(Holdable stuckObject)
    {
        GameObject trackObject = new GameObject("stuckObject",  typeof(SpriteRenderer));
        trackObject.transform.parent = CharComponents.CharacterRigidBodyCapsuleColliderHitBox.transform;
        trackObject.transform.position = stuckObject.transform.position;

        do
        {
            stuckObject.transform.position = trackObject.transform.position;

            yield return new WaitForEndOfFrame();
        }
        while (!stuckObject.IsDestroyed() && stuckObject.StuckedToCollider?.GetComponent<AbstractCharacterComponent>()?.CharComponents == CharComponents);

        Destroy(trackObject);
    }
}
