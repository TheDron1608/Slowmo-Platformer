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
        StuckTrackTarget.CreateTrack(obj, CharComponents.CharacterRigidBodyCapsuleCollider.transform);
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

            RemoveStuckedObject(stuckObject);
            stuckObject.StuckedToCollider = null;

            if (stuckObject.TryGetComponent(out Rigidbody2D stuckObjectRigidBody))
            {
                stuckObjectRigidBody.linearVelocity = VectorMath.GetAngleToAsNormalizedVec2(CharComponents.Center.transform.position, stuckObject.transform.position) * RemoveObjectVelocity * stuckObject.ThrowForceMultiplier;
                stuckObjectRigidBody.angularVelocity = RemoveObjectMaxRandomAngularVelocity * (UnityEngine.Random.value * 2 - 1);
            }
        }
    }
}
