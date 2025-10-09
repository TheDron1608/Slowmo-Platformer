using System.Collections.Generic;

internal interface IStuckToObject
{
    public List<Holdable> StuckedObjects
    {
        get;
    }

    public void AddStuckedObject(Holdable obj);
    public void RemoveStuckedObject(Holdable obj);

    public void RemoveAllStuckedObjects();
}
