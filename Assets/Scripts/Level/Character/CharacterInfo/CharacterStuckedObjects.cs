using System.Collections.Generic;
using UnityEngine;

public class CharacterStuckedObjects : AbstractCharacterComponent, IStuckToObject
{
    public float RemoveObjectVelocity = 3.5f;
    public float RemoveObjectMaxRandomAngularVelocity = 360f;

    private List<IStuckableObject> _stuckedObjects = new();

    public List<IStuckableObject> StuckedObjects
    {
        get => _stuckedObjects;
    }

    public void AddStuckedObject(IStuckableObject obj)
    {
        _stuckedObjects.Add(obj);
        StuckTrackTarget.CreateTrack(obj, CharComponents.CharacterRigidBodyCapsuleCollider.transform);
    }

    public void RemoveStuckedObject(IStuckableObject obj)
    {
        _stuckedObjects.Remove(obj);
    }

    public void RemoveAllStuckedObjects()
    {
        for (int i = 0; i < _stuckedObjects.Count; i++)
        {
            IStuckableObject stuckObject = _stuckedObjects[i];

            RemoveStuckedObject(stuckObject);
            stuckObject.StuckedToCollider = null;

            if ((stuckObject as MonoBehaviour).TryGetComponent(out Rigidbody2D stuckObjectRigidBody))
            {
                (stuckObject as MonoBehaviour).TryGetComponent(out Holdable holdable);
                stuckObjectRigidBody.linearVelocity = 
                    VectorMath.GetAngleToAsNormalizedVec2(CharComponents.Center.transform.position, (stuckObject as MonoBehaviour).transform.position) * RemoveObjectVelocity * (holdable?.ThrowForceMultiplier ?? 1f);
                stuckObjectRigidBody.angularVelocity = RemoveObjectMaxRandomAngularVelocity * (UnityEngine.Random.value * 2 - 1);
            }
        }
    }
}
