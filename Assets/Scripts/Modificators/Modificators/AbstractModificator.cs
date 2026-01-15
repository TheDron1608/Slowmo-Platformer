using UnityEngine;

public abstract class AbstractModificator : MonoBehaviour
{
    public ModificatorIcon IconInstance;
    public ModificatorCard CardInstance;

    private ModificatorIcon _currentIcon;

    public ModificatorIcon CurrentIcon
    {
        get => _currentIcon;
        set => _currentIcon = value;
    }

    public virtual bool GetEqualType(AbstractModificator other)
    {
        return GetType() == other.GetType();
    }

    public void TryTriggerIconAnimation()
    {
        CurrentIcon?.TriggerAnimation();
    }

    public virtual void OnModificatorAdded()
    {

    }

    public virtual void OnModificatorRemoved()
    {

    }

    public virtual void OnLevelPreGenerated()
    {
        LayerManager.Instance.OnObjectSpawned += OnObjectSpawned;
    }

    public virtual void OnLevelGenerated()
    {

    }

    public virtual void OnLevelFinished()
    {
        LayerManager.Instance.OnObjectSpawned -= OnObjectSpawned;
    }

    public virtual void OnModificatorChoiseStarted()
    {

    }

    public virtual void OnModificatorChoiseFinished()
    {

    }

    protected virtual void OnObjectSpawned(object sender, GameObject e)
    {

    }

    private void OnDestroy()
    {
        OnModificatorRemoved(); 
    }
}