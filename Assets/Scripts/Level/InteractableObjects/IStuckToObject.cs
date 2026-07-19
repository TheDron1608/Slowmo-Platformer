using System.Collections.Generic;
using UnityEngine;

public interface IStuckToObject
{
    public List<IStuckableObject> StuckedObjects
    {
        get;
    }

    public void AddStuckedObject(IStuckableObject obj);
    public void RemoveStuckedObject(IStuckableObject obj);

    public void RemoveAllStuckedObjects();
}
