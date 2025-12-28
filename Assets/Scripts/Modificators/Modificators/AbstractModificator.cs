using UnityEngine;

public abstract class AbstractModificator : MonoBehaviour
{
    public ModificatorIcon IconInstance;
    public ModificatorCard CardInstance;

    public virtual bool GetEqualType(AbstractModificator other)
    {
        return GetType() == other.GetType();
    }

    public virtual void OnModificatorAdded()
    {

    }

    public virtual void OnModificatorRemoved()
    {

    }

    public virtual void OnLevelPreGenerated()
    {

    }

    public virtual void OnLevelGenerated()
    {

    }

    public virtual void OnLevelFinished()
    {

    }

    public virtual void OnModificatorChoiseStarted()
    {

    }

    public virtual void OnModificatorChoiseFinished()
    {

    }
}