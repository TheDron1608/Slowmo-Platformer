using System;
using System.Collections.Generic;
using UnityEngine;
using static CharacterVisual;

public class CharacterStuckedObjects : AbstractCharacterComponent
{
    public float RemoveObjectVelocity = 5f;

    private List<Holdable> _stuckedObjects = new();

    public List<Holdable> StuckedObjects
    {
        get => _stuckedObjects;
        set => _stuckedObjects = value;
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
                stuckObjectRigidBody.linearVelocity += VectorMath.GetAngleToAsNormalizedVec2(CharComponents.Center.transform.position, stuckObject.transform.position) * RemoveObjectVelocity * stuckObject.ThrowForceMultiplier;
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
                stuckObjectRigidBody.linearVelocity += (VectorMath.GetAngleToAsNormalizedVec2(CharComponents.Center.transform.position, stuckObject.transform.position) + direction * 3).normalized * RemoveObjectVelocity * stuckObject.ThrowForceMultiplier;
            }
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, OnBusyStateChangedEventArgs e)
    {
        if (e.NewState == CharacterPart.CharacterPartBusyStates.ROLL)
        {
            RemoveAllStuckedObjects();
        }
        else if (e.NewState == CharacterPart.CharacterPartBusyStates.FALLEN_ON_FLOOR || e.NewState == CharacterPart.CharacterPartBusyStates.FALLING_IN_AIR)
        {
            RemoveAllStuckedObjects(CharComponents.CharacterRigidBody.linearVelocity.normalized);
        }
    }
}
