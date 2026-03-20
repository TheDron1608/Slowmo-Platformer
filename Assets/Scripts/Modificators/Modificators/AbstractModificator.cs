using UnityEngine;

public abstract class AbstractModificator : MonoBehaviour
{
    public enum ModificatorTypes
    {
        POSITIVE,
        NEGATIVE,
        NEUTRAL
    }

    public float ModificatorPrice = 0f;

    public ModificatorTypes ModificatorType;
    public ModificatorIcon IconInstance;
    public ModificatorCard CardInstance;
    public bool Multiplierable = false;
    public float ModificatorMultiplier = 1f;

    private ModificatorIcon _currentIcon;
    private bool _disabledModificator = false;

    public bool DisabledModificator
    {
        get => _disabledModificator;
        set
        {
            if (value == _disabledModificator) return;
            _disabledModificator = value;

            if (_disabledModificator)
            {
                OnModificatorAdded();
            }
            else
            {
                OnModificatorRemoved();
            }

            if (_currentIcon != null) _currentIcon.DisabledIcon = value;
        }
    }

    public ModificatorIcon CurrentIcon
    {
        get => _currentIcon;
        set => _currentIcon = value;
    }

    public virtual bool GetEqualType(AbstractModificator other)
    {
        return 
            GetType() == other.GetType() && 
            ((!Multiplierable && !other.Multiplierable) || ModificatorMultiplier == other.ModificatorMultiplier);
    }

    public void TryTriggerIconAnimation()
    {
        CurrentIcon?.TriggerAnimation();
    }

    public virtual void OnModificatorAdded()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnObjectSpawned += OnObjectSpawned;
        }
    }

    public virtual void OnModificatorRemoved()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnObjectSpawned -= OnObjectSpawned;
        }
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
        if (!DisabledModificator)
        {
            OnModificatorRemoved(); 
        }
    }
}